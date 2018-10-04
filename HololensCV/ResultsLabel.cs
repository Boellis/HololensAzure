using System.Collections.Generic;
using UnityEngine;

public class ResultsLabel : MonoBehaviour {
    public static ResultsLabel instance;

    public GameObject cursor;

    public Transform labelPrefab;

    [HideInspector]
    public Transform lastLabelPlaced;

    [HideInspector]
    public TextMesh lastLabelPlacedText;

    private void Awake(){
        instance = this;
    }

    //Instantiate a Label in the appropriate location relative to the Main Camera
    public void CreateLabel(){
        lastLabelPlaced = Instantiate(labelPrefab, cursor.transform.position,transform.rotation);
        
        //Change the text of the label to show that has been placed
        //Set the final text of the last label in a different var
        lastLabelPlacedText.text = "Analysing...";
    }

    //Set the tags as Text of the last label created
    public void SetTagsToLastLabel(Dictionary<string,float> tagsDictionary){
        lastLabelPlacedText = lastLabelPlaced.GetComponent<TextMesh>();

        //Go through all tags received and set them as text of the label
        lastLabelPlacedText.text = "I see: \n";

        foreach(KeyValuePair<string, float> tag in tagsDictionary){
            lastLabelPlacedText.text += tag.Key ", Confidence: " + tag.Value.ToString("0.00 \n");
        }
    }    
}