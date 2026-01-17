using UnityEngine;

public class InstrumentSelector : MonoBehaviour
{
    public static InstrumentSelector I { get; private set; }

    public InstrumentIdentity Current { get; private set; }

    void Awake()
    {
        I = this;
    }

    public bool HasSelection => Current != null;

    public void Select(InstrumentIdentity instrument)
    {
        Current = instrument;
        Debug.Log("Selected instrument: " + instrument.type);
    }

    public void ClearIfSame(InstrumentIdentity instrument)
    {
        if (Current == instrument)
        {
            Current = null;
            Debug.Log("Instrument unselected: " + instrument.type);
        }
    }
}
