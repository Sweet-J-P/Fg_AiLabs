using UnityEngine;

public class SimpleMoodChange : MonoBehaviour
{
    
    [SerializeField] private Transform[] eyebrows;
    [SerializeField] private bool bStartSad;

    private void Start()
    {
        if(bStartSad)
            NewMood();
    }
    
    public void NewMood()
    {
        foreach (var eyebrow in eyebrows)
        {
            float rotation = eyebrow.localEulerAngles.z;
            rotation *= -1f;
            eyebrow.localEulerAngles = new Vector3(0f, 0f, rotation);
        }
    }
}
