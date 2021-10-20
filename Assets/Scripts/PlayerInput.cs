// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/PlayerInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInput : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInput"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""da2faa08-77b0-4514-b4ea-ad7f6514f087"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Button"",
                    ""id"": ""415755bb-3b64-4157-91ac-af730785df04"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Fast"",
                    ""type"": ""Button"",
                    ""id"": ""f1221884-3ddc-449e-9105-868941760fdc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)""
                },
                {
                    ""name"": ""Careful"",
                    ""type"": ""Button"",
                    ""id"": ""4ae841a6-cf7c-4749-8fa9-dffddb794c4c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)""
                },
                {
                    ""name"": ""Use"",
                    ""type"": ""Button"",
                    ""id"": ""6b1c08d4-8e7e-4e81-9544-393a4db8e40e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)""
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""a7161fdb-9ffe-487f-88c4-443ca4ca108b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)""
                },
                {
                    ""name"": ""ScreenPointer"",
                    ""type"": ""Value"",
                    ""id"": ""b793a0b4-cfe8-4da5-88fc-4ef0a5233bb8"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Accept"",
                    ""type"": ""Button"",
                    ""id"": ""30e58e82-0a2b-401a-9b09-057a30eb18bc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)""
                },
                {
                    ""name"": ""Alternative"",
                    ""type"": ""Button"",
                    ""id"": ""5ba2e466-e8c3-4f58-8d57-380b95c940e5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)""
                },
                {
                    ""name"": ""Back"",
                    ""type"": ""Button"",
                    ""id"": ""6ba2d9e0-008e-4a06-96af-95b208de027d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press(behavior=1)""
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""b2cca064-43de-48aa-b97f-a315fc5a63b7"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""520d1699-1973-43fb-ae38-d7efefe77fa1"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""1e9ae18e-ecbd-4e5b-b5f2-0f1ae106929a"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""63506f00-c2cf-4c43-9cda-4608191052a2"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""91dbd4a0-bb9f-47c0-a064-c6dc280a42ac"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""6f0e1f68-e26b-42e6-8379-962d457d6023"",
                    ""path"": ""<Keyboard>/shift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fast"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1ca9e9e6-a13e-411d-881f-4e7cb24f6d77"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Careful"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b53e8e0b-8dd4-402c-a133-a9809f1e92d8"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Use"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2c5b9a7a-696b-4fed-a1e6-c344fd88e743"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""24f2946a-f91d-4e32-9831-d2ed64d831e4"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ScreenPointer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b321b7af-88ef-485b-89ae-24e3f9903015"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Accept"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""202e309e-f097-4fcc-b8e8-b640ba657dac"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Alternative"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c6a56ffd-2629-4063-9083-90531e3c5cbc"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Back"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Fast = m_Player.FindAction("Fast", throwIfNotFound: true);
        m_Player_Careful = m_Player.FindAction("Careful", throwIfNotFound: true);
        m_Player_Use = m_Player.FindAction("Use", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_ScreenPointer = m_Player.FindAction("ScreenPointer", throwIfNotFound: true);
        m_Player_Accept = m_Player.FindAction("Accept", throwIfNotFound: true);
        m_Player_Alternative = m_Player.FindAction("Alternative", throwIfNotFound: true);
        m_Player_Back = m_Player.FindAction("Back", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Player
    private readonly InputActionMap m_Player;
    private IPlayerActions m_PlayerActionsCallbackInterface;
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Fast;
    private readonly InputAction m_Player_Careful;
    private readonly InputAction m_Player_Use;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_ScreenPointer;
    private readonly InputAction m_Player_Accept;
    private readonly InputAction m_Player_Alternative;
    private readonly InputAction m_Player_Back;
    public struct PlayerActions
    {
        private @PlayerInput m_Wrapper;
        public PlayerActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Fast => m_Wrapper.m_Player_Fast;
        public InputAction @Careful => m_Wrapper.m_Player_Careful;
        public InputAction @Use => m_Wrapper.m_Player_Use;
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @ScreenPointer => m_Wrapper.m_Player_ScreenPointer;
        public InputAction @Accept => m_Wrapper.m_Player_Accept;
        public InputAction @Alternative => m_Wrapper.m_Player_Alternative;
        public InputAction @Back => m_Wrapper.m_Player_Back;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Fast.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnFast;
                @Fast.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnFast;
                @Fast.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnFast;
                @Careful.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCareful;
                @Careful.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCareful;
                @Careful.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCareful;
                @Use.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnUse;
                @Use.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnUse;
                @Use.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnUse;
                @Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @ScreenPointer.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnScreenPointer;
                @ScreenPointer.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnScreenPointer;
                @ScreenPointer.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnScreenPointer;
                @Accept.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAccept;
                @Accept.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAccept;
                @Accept.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAccept;
                @Alternative.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAlternative;
                @Alternative.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAlternative;
                @Alternative.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAlternative;
                @Back.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBack;
                @Back.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBack;
                @Back.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnBack;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Fast.started += instance.OnFast;
                @Fast.performed += instance.OnFast;
                @Fast.canceled += instance.OnFast;
                @Careful.started += instance.OnCareful;
                @Careful.performed += instance.OnCareful;
                @Careful.canceled += instance.OnCareful;
                @Use.started += instance.OnUse;
                @Use.performed += instance.OnUse;
                @Use.canceled += instance.OnUse;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @ScreenPointer.started += instance.OnScreenPointer;
                @ScreenPointer.performed += instance.OnScreenPointer;
                @ScreenPointer.canceled += instance.OnScreenPointer;
                @Accept.started += instance.OnAccept;
                @Accept.performed += instance.OnAccept;
                @Accept.canceled += instance.OnAccept;
                @Alternative.started += instance.OnAlternative;
                @Alternative.performed += instance.OnAlternative;
                @Alternative.canceled += instance.OnAlternative;
                @Back.started += instance.OnBack;
                @Back.performed += instance.OnBack;
                @Back.canceled += instance.OnBack;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnFast(InputAction.CallbackContext context);
        void OnCareful(InputAction.CallbackContext context);
        void OnUse(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnScreenPointer(InputAction.CallbackContext context);
        void OnAccept(InputAction.CallbackContext context);
        void OnAlternative(InputAction.CallbackContext context);
        void OnBack(InputAction.CallbackContext context);
    }
}
