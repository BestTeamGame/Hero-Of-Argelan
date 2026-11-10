using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts.PlayerScripts
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(PlayerSkillController))]
    [RequireComponent(typeof(PlayerConsumables))]
    public class PlayerInputController : MonoBehaviour
    {
        private PlayerMovement movement;
        private PlayerCombat combat;
        private PlayerSkillController skills;
        private PlayerConsumables consumables;

        private InputSystem_Actions input;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            combat = GetComponent<PlayerCombat>();
            skills = GetComponent<PlayerSkillController>();
            consumables = GetComponent<PlayerConsumables>();

            input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            input.Player.Enable();

            input.Player.Move.performed += OnMove;
            input.Player.Move.canceled += OnMove;

            input.Player.Jump.performed += OnJump;
            
            input.Player.Interact.performed += OnInteract;

            input.Player.Attack1.performed += OnAttack1;
            input.Player.Attack2.performed += OnAttack2;

            input.Player.Skill.performed += OnSkill;

            input.Player.HealthPotion.performed +=
                OnHealthPotion;

            input.Player.ManaPotion.performed +=
                OnManaPotion;
        }

        private void OnDisable()
        {
            input.Player.Move.performed -= OnMove;
            input.Player.Move.canceled -= OnMove;

            input.Player.Jump.performed -= OnJump;

            input.Player.Interact.performed -= OnInteract;

            input.Player.Attack1.performed -= OnAttack1;
            input.Player.Attack2.performed -= OnAttack2;

            input.Player.Skill.performed -= OnSkill;

            input.Player.HealthPotion.performed -=
                OnHealthPotion;

            input.Player.ManaPotion.performed -=
                OnManaPotion;

            input.Player.Disable();

            movement.SetMoveInput(Vector2.zero);
        }

        private void OnDestroy()
        {
            input?.Dispose();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            movement.SetMoveInput(
                context.ReadValue<Vector2>()
            );
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            movement.RequestJump();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            // TODO: метод для взаимодействия с NPC / объектами
        }
        
        private void OnAttack1(InputAction.CallbackContext context)
        {
            combat.TryAttackWeapon1();
        }

        private void OnAttack2(InputAction.CallbackContext context)
        {
            combat.TryAttackWeapon2();
        }

        private void OnSkill(InputAction.CallbackContext context)
        {
            skills.TryUseSkill();
        }

        private void OnHealthPotion(InputAction.CallbackContext context)
        {
            consumables.UseHealthPotion();
        }

        private void OnManaPotion(InputAction.CallbackContext context)
        {
            consumables.UseManaPotion();
        }
    }
}