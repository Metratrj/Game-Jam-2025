using UnityEngine;
using TMPro;

public class ShroomCollection : MonoBehaviour
{
    private int Shroom = 0;

    public TextMeshProUGUI shroomText;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "ScoreShroom")
        {
            Shroom++;
            shroomText.text = "Magic Shrooms: " + Shroom.ToString();
            Debug.Log(Shroom);
            Destroy(other.gameObject);
        }
    }
}
