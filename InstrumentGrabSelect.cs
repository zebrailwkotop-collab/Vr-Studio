using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(Interactable))]
public class InstrumentGrabSelect : MonoBehaviour
{
    public InstrumentIdentity identity;
    private Interactable _interactable;

    void Awake()
    {
        _interactable = GetComponent<Interactable>();
        if (identity == null) identity = GetComponent<InstrumentIdentity>();
    }

    void OnEnable()
    {
        _interactable.onAttachedToHand += OnGrabbed;
        _interactable.onDetachedFromHand += OnReleased;
    }

    void OnDisable()
    {
        _interactable.onAttachedToHand -= OnGrabbed;
        _interactable.onDetachedFromHand -= OnReleased;
    }

    private void OnGrabbed(Hand hand)
    {
        if (identity == null) return;
        InstrumentSelector.I.Select(identity);
    }

    private void OnReleased(Hand hand)
    {
        if (identity == null) return;
        InstrumentSelector.I.ClearIfSame(identity);
    }
}
