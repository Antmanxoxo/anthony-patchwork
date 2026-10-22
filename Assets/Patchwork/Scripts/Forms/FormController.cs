using System.Collections.Generic;
using UnityEngine;

namespace Patchwork
{
    /// <summary>
    /// Owns the current form and performs transformations safely:
    ///  - refuses locked forms,
    ///  - defers while an ability is mid-action (e.g. swinging on yarn),
    ///  - refuses if the new body would not fit (prevents getting stuck in walls/grates),
    ///  - keeps the feet position fixed so the player never teleports or falls through floors,
    ///  - clears temporary movement state so no ability stays active.
    /// </summary>
    public class FormController
    {
        readonly PatchworkPlayer player;
        readonly Dictionary<FormType, FormAbility> abilities = new Dictionary<FormType, FormAbility>();

        public FormType Current { get; private set; }
        public FormDefinition Definition { get { return FormLibrary.Get(Current); } }
        public FormAbility Ability { get { return abilities[Current]; } }

        bool hasPending;
        FormType pending;

        public bool HasPending { get { return hasPending; } }
        public FormType Pending { get { return pending; } }

        public FormController(PatchworkPlayer player)
        {
            this.player = player;
            abilities[FormType.Water] = new WaterAbility();
            abilities[FormType.Yarn] = new YarnAbility();
            abilities[FormType.Wind] = new WindAbility();
            foreach (var a in abilities.Values) a.Init(player);
        }

        public T Get<T>() where T : FormAbility
        {
            foreach (var a in abilities.Values)
                if (a is T) return (T)a;
            return null;
        }

        /// <summary>Sets the starting form with no effects. Falls back to any form that fits.</summary>
        public void Initialise(FormType start)
        {
            if (!GameState.IsFormUnlocked(start)) start = FirstUnlocked();
            Current = start;
            ApplyDefinition();
            Ability.OnEnter();
            player.Visuals.SetForm(Current, false);
        }

        public bool IsAvailable(FormType form)
        {
            return GameState.IsFormUnlocked(form);
        }

        /// <summary>Ask to transform. Returns true if the change happened or was queued.</summary>
        public bool Request(FormType target)
        {
            if (target == Current && !hasPending) return true;

            if (!IsAvailable(target))
            {
                Fail(FormLibrary.Get(target).DisplayName + " isn't sewn yet.");
                return false;
            }

            if (Ability.BlocksTransform)
            {
                if (target == Current) { hasPending = false; return true; }
                hasPending = true;
                pending = target;
                player.Game.ShowToast(Ability.BlockReason);
                return true;
            }

            return TryApply(target);
        }

        public void Tick(float dt)
        {
            if (hasPending && !Ability.BlocksTransform)
            {
                hasPending = false;
                if (pending != Current) TryApply(pending);
            }
        }

        /// <summary>Immediately cancels abilities and any queued transformation (respawn/scene change).</summary>
        public void CancelAll()
        {
            hasPending = false;
            Ability.ForceCancel();
            player.Motor.ExternalControl = false;
            player.Motor.ResetTransientState();
        }

        bool TryApply(FormType target)
        {
            var def = FormLibrary.Get(target);
            if (!HasRoomFor(def, target == FormType.Water))
            {
                Fail("Too cramped to change here. Find more space.");
                return false;
            }

            Ability.OnExit();
            Current = target;
            ApplyDefinition();
            Ability.OnEnter();

            GameState.CurrentForm = Current;

            // Keep momentum but drop anything that only made sense for the old form.
            var m = player.Motor;
            m.ResetTransientState();
            m.ExternalControl = false;
            if (m.Velocity.y > 4f) m.Velocity = new Vector3(m.Velocity.x, 4f, m.Velocity.z);

            player.Visuals.SetForm(Current, true);
            Sfx.Play(SfxId.Transform, 0.7f);
            TransformBurst.Spawn(m.Center, def.PrimaryColor, def.SecondaryColor, 16, 0.9f);
            return true;
        }

        void ApplyDefinition()
        {
            var def = Definition;
            player.Motor.Params = def;
            player.Motor.ApplyControllerShape(def);
        }

        /// <summary>Would a capsule of this form's size fit at the player's feet right now?</summary>
        public bool HasRoomFor(FormDefinition def, bool ignoreGrates)
        {
            var controller = player.Motor.Controller;
            float r = def.ControllerRadius * 0.95f;
            Vector3 feet = player.transform.position;
            Vector3 p1 = feet + Vector3.up * (def.ControllerRadius + 0.12f);
            Vector3 p2 = feet + Vector3.up * Mathf.Max(def.ControllerHeight - def.ControllerRadius, def.ControllerRadius + 0.12f);

            Collider[] hits = Physics.OverlapCapsule(p1, p2, r, ~(1 << 2), QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h == null || h == controller) continue;
                if (ignoreGrates && WaterGrate.IsGrate(h)) continue;
                return false;
            }
            return true;
        }

        FormType FirstUnlocked()
        {
            foreach (var f in FormLibrary.AllForms)
                if (GameState.IsFormUnlocked(f)) return f;
            return FormType.Yarn;
        }

        void Fail(string message)
        {
            Sfx.Play(SfxId.TransformFail, 0.5f);
            player.Game.ShowToast(message);
            player.Visuals.PunchScale(new Vector3(1.15f, 0.9f, 1.15f));
        }
    }
}
