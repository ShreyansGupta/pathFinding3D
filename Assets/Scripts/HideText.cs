using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideText : MonoBehaviour
{
    private Text txt;

    public float wait = 30f;
    // Start is called before the first frame update
    void Start()
    {
        txt = GetComponentInChildren<Text>();
        StartCoroutine(Hide());
    }

    // Update is called once per frame
    IEnumerator Hide()
    {
        yield return new WaitForSeconds(wait);
        txt.text = "Controls: (R or G) + number{1,2,3,4,5,6}";
    }
}
