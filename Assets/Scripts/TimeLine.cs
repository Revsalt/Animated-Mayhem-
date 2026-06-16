using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;

public class TimeLine : MonoBehaviour
{
    public static TimeLine Instance;

    public GameObject number;
    public GameObject numberVisualization;
    public Vector2 minnmaxFrames = Vector2.zero;

    public KeyFrameSlot SelectedFrameSlot = null;
    public GameObject Pointer;

    bool isPlaying = false;
    void Start()
    {
        Instance = this;

        UpdateTimeLineNumber(new Vector2(minnmaxFrames.x, minnmaxFrames.y + 1));
    }

    public void SaveCurrentFrame()
    {
        if (SelectedFrameSlot == null)
            return;

        KeyFrame key = new KeyFrame();
        key.positions = EditorController.Instance.GetModelPositions();
        key.rotaitons = EditorController.Instance.GetModelRotations();
        SelectedFrameSlot.AddKeyFrame(key);
    }

    public void LoadNewTimeLine(KeyFrame[] newFrames)
    {
        List<KeyFrameSlot> _frames = new List<KeyFrameSlot>();
        foreach (Transform child in numberVisualization.transform)
        {
            _frames.Add(child.gameObject.GetComponent<KeyFrameSlot>());
        }

        foreach (var olditem in _frames)
        {
            if (olditem.myKeyFrame != null)
                olditem.RemoveKeyFrame();

            foreach (var newitem in newFrames)
            {
                Debug.Log(Convert.ToInt32(olditem.GetComponent<Text>().text) + " " + newitem.KeyFrameNumber);
                if (Convert.ToInt32(olditem.GetComponent<Text>().text) == newitem.KeyFrameNumber)
                {
                    KeyFrame key = new KeyFrame();
                    key.positions = newitem.positions;
                    key.KeyFrameNumber = newitem.KeyFrameNumber;
                    key.rotaitons = newitem.rotaitons;
                    olditem.AddKeyFrame(key);
                }
            }
        }
    }

    public KeyFrame[] GetKeyFrames()
    {
        List<KeyFrame> frames = new List<KeyFrame>();
        foreach (Transform child in numberVisualization.transform)
        {
            if (child.gameObject.GetComponent<KeyFrameSlot>().myKeyFrame != null)
                frames.Add(child.gameObject.GetComponent<KeyFrameSlot>().myKeyFrame);
        }

        return frames.ToArray();
    }

    public void Play()
    {
        isPlaying = true;

        List<GameObject> frames = new List<GameObject>();
        foreach (Transform child in numberVisualization.transform)
        {
            frames.Add(child.gameObject);
        }

        StopAllCoroutines();
        StartCoroutine(Play(frames));

    }

    public void Pause()
    {
        StopAllCoroutines();
        isPlaying = false;
        EditorController.Instance.characterValues.key = null;
    }


    IEnumerator Play(List<GameObject> frames)
    {
        List<KeyFrameSlot> keyedFrames = new List<KeyFrameSlot>();
        List<int> durationBetweenFrames = new List<int>();

        for (int i = 0; i < frames.Count - 1; i++)
        {
            if (frames[i].GetComponent<KeyFrameSlot>().myKeyFrame != null)
            {
                keyedFrames.Add(frames[i].GetComponent<KeyFrameSlot>());
                durationBetweenFrames.Add(0);

                if (keyedFrames.Count == 1)
                    EditorController.Instance.characterValues.PlayKeyFrame(frames[i].GetComponent<KeyFrameSlot>().myKeyFrame, true, 0);
            }

            if (durationBetweenFrames.Count > 0)
            {
                durationBetweenFrames[durationBetweenFrames.Count - 1]++;
            }
        }

        if (keyedFrames.Count == 0)
            yield return null;

        for (int i = 0; i < frames.Count - 1; i++)
        {
            foreach (var item in keyedFrames)
            {
                if (item == frames[i].GetComponent<KeyFrameSlot>())
                {
                    int keyindex = keyedFrames.IndexOf(item);

                    if (keyindex + 1 <= keyedFrames.Count - 1)
                        EditorController.Instance.characterValues.PlayKeyFrame(keyedFrames[keyindex + 1].myKeyFrame, false, durationBetweenFrames[keyindex]);
                }
            }

            Pointer.transform.position = frames[i].transform.position + Vector3.up * 8;
            yield return new WaitForSeconds(.1f);
        }

        isPlaying = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete) && SelectedFrameSlot != null)
            SelectedFrameSlot.RemoveKeyFrame();

        if (!isPlaying && SelectedFrameSlot != null)
            Pointer.transform.position = SelectedFrameSlot.transform.position + Vector3.up * 8;
    }

    void UpdateTimeLineNumber(Vector2 timeLineLimit)
    {
        foreach (Transform child in numberVisualization.transform)
        {
            Destroy(child.gameObject);
        }

        for (float i = timeLineLimit.x; i < timeLineLimit.y; i++)
        {
            GameObject g = Instantiate(number, numberVisualization.transform);
            g.GetComponent<Text>().text = i.ToString();
        }
    }
}
