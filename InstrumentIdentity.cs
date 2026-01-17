using UnityEngine;

public enum InstrumentType { Guitar, Bass, Drums, Keys }

public class InstrumentIdentity : MonoBehaviour
{
    public InstrumentType type;
    public AudioSource source;
}
