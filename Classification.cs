using UnityEngine;
using TensorFlow;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class BoxOutline
{
    public float YMin { get; set; } = 0;
    public float XMin { get; set; } = 0;
    public float YMax { get; set; } = 0;
    public float XMax { get; set; } = 0;
    public string Label { get; set; }
    public float Score { get; set; }
}

public class Classification : MonoBehaviour
{

    [Header("Constants")]
    private const int INPUT_SIZE = 224;
    private const int IMAGE_MEAN = 117;
    private const float IMAGE_STD = 1;
    private const string INPUT_TENSOR = "input";
    private const string OUTPUT_TENSOR = "output";

    [Header("Inspector Stuff")]
    public CameraFeedBehavior camFeed;
    public TextAsset labelMap;
    public TextAsset model;
    public AudioSource audioSource;
    public MessageBehavior messageBehavior;

    private TFGraph graph;
    private TFSession session;
    private string[] labels;

    private List<BoxOutline> boxOutlines;
    private static Texture2D boxOutlineTexture;
    private Vector2 backgroundSize;
    private Vector2 backgroundOrigin;
    private static GUIStyle labelStyle;

    // Use this for initialization
    void Start()
    {
#if UNITY_ANDROID
        TensorFlowSharp.Android.NativeBinding.Init();
#endif
        //load labels into string array
        labels = labelMap.ToString().Split('\n');
        //load graph
        graph = new TFGraph();
        graph.Import(model.bytes);
        session = new TFSession(graph);
    }

    private void Update()
    {
        //process image on click or touch
        if (Input.GetMouseButtonDown(0))
        {
            ProcessImage();
        }
    }

    void ProcessImage()
    {
        //pass in input tensor
        var tensor = TransformInput(camFeed.GetImage(), INPUT_SIZE, INPUT_SIZE);
        var runner = session.GetRunner();
        runner.AddInput(graph[INPUT_TENSOR][0], tensor).Fetch(graph[OUTPUT_TENSOR][0]);
        var output = runner.Run();
        //put results into one dimensional array
        float[] probs = ((float[][])output[0].GetValue(jagged: true))[0];
        //get max value of probabilities and find its associated label index
        float maxValue = probs.Max();
        int maxIndex = probs.ToList().IndexOf(maxValue);
        //print label with highest probability
        string label = labels[maxIndex];
        print(label);

        audioSource.clip = Resources.Load("imageNetSounds/" + label) as AudioClip;
        audioSource.Play();
        messageBehavior.ShowMessage(label);
    }

    //stole from https://github.com/Syn-McJ/TFClassify-Unity
    public static TFTensor TransformInput(Color32[] pic, int width, int height)
    {
        float[] floatValues = new float[width * height * 3];

        for (int i = 0; i < pic.Length; ++i)
        {
            var color = pic[i];

            floatValues[i * 3 + 0] = (color.r - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 1] = (color.g - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 2] = (color.b - IMAGE_MEAN) / IMAGE_STD;
        }

        TFShape shape = new TFShape(1, width, height, 3);

        return TFTensor.FromBuffer(shape, floatValues, 0, floatValues.Length);
    }
    public void OnGUI()
    {
        if (this.boxOutlines != null && this.boxOutlines.Any())
        {
            foreach (var outline in this.boxOutlines)
            {
                DrawBoxOutline(outline);
            }
        }
    }
    //private async void TFDetect()
    //{
    //    UpdateBackgroundOrigin();

    //    var snap = TakeTextureSnap();
    //    var scaled = Scale(snap, detectImageSize);
    //    var rotated = await RotateAsync(scaled.GetPixels32(), scaled.width, scaled.height);
    //    this.boxOutlines = await this.detector.DetectAsync(rotated);

    //    Destroy(snap);
    //    Destroy(scaled);
    //}


    //private void UpdateBackgroundOrigin()
    //{
    //    var backgroundPosition = this.background.transform.position;
    //    this.backgroundOrigin = new Vector2(backgroundPosition.x - this.backgroundSize.x / 2,
    //                                        backgroundPosition.y - this.backgroundSize.y / 2);
    //}


    private void DrawBoxOutline(BoxOutline outline)
    {
        var xMin = outline.XMin * this.backgroundSize.x + this.backgroundOrigin.x;
        var xMax = outline.XMax * this.backgroundSize.x + this.backgroundOrigin.x;
        var yMin = outline.YMin * this.backgroundSize.y + this.backgroundOrigin.y;
        var yMax = outline.YMax * this.backgroundSize.y + this.backgroundOrigin.y;

        DrawRectangle(new Rect(xMin, yMin, xMax - xMin, yMax - yMin), 4, Color.red);
        DrawLabel(new Rect(xMin + 10, yMin + 10, 200, 20), $"{outline.Label}: {(int)(outline.Score * 100)}%");
    }


    public static void DrawRectangle(Rect area, int frameWidth, Color color)
    {
        // Create a one pixel texture with the right color
        if (boxOutlineTexture == null)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            boxOutlineTexture = texture;
        }

        Rect lineArea = area;
        lineArea.height = frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Top line

        lineArea.y = area.yMax - frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Bottom line

        lineArea = area;
        lineArea.width = frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Left line

        lineArea.x = area.xMax - frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Right line
    }


    private static void DrawLabel(Rect position, string text)
    {
        if (labelStyle == null)
        {
            var style = new GUIStyle();
            style.fontSize = 50;
            style.normal.textColor = Color.red;
            labelStyle = style;
        }

        GUI.Label(position, text, labelStyle);
    }
}
public class Detector
{
    private static int IMAGE_MEAN = 117;
    private static float IMAGE_STD = 1;

    // Minimum detection confidence to track a detection.
    private static float MINIMUM_CONFIDENCE = 0.6f;

    private int inputSize;
    private TFGraph graph;
    private string[] labels;

    public Detector(byte[] model, string[] labels, int inputSize)
    {
#if UNITY_ANDROID
        TensorFlowSharp.Android.NativeBinding.Init();
#endif
        this.labels = labels;
        this.inputSize = inputSize;
        this.graph = new TFGraph();
        this.graph.Import(new TFBuffer(model));
    }


    public Task<List<BoxOutline>> DetectAsync(Color32[] data)
    {
        return Task.Run(() =>
        {
            using (var session = new TFSession(this.graph))
            using (var tensor = TransformInput(data, this.inputSize, this.inputSize))
            {
                var runner = session.GetRunner();
                runner.AddInput(this.graph["image_tensor"][0], tensor)
                      .Fetch(this.graph["detection_boxes"][0],
                             this.graph["detection_scores"][0],
                             this.graph["detection_classes"][0],
                             this.graph["num_detections"][0]);
                var output = runner.Run();

                var boxes = (float[,,])output[0].GetValue(jagged: false);
                var scores = (float[,])output[1].GetValue(jagged: false);
                var classes = (float[,])output[2].GetValue(jagged: false);

                foreach (var ts in output)
                {
                    ts.Dispose();
                }

                return GetBoxes(boxes, scores, classes, MINIMUM_CONFIDENCE);
            }
        });
    }


    public static TFTensor TransformInput(Color32[] pic, int width, int height)
    {
        byte[] floatValues = new byte[width * height * 3];

        for (int i = 0; i < pic.Length; ++i)
        {
            var color = pic[i];

            floatValues[i * 3 + 0] = (byte)((color.r - IMAGE_MEAN) / IMAGE_STD);
            floatValues[i * 3 + 1] = (byte)((color.g - IMAGE_MEAN) / IMAGE_STD);
            floatValues[i * 3 + 2] = (byte)((color.b - IMAGE_MEAN) / IMAGE_STD);
        }

        TFShape shape = new TFShape(1, width, height, 3);

        return TFTensor.FromBuffer(shape, floatValues, 0, floatValues.Length);
    }


    private List<BoxOutline> GetBoxes(float[,,] boxes, float[,] scores, float[,] classes, double minScore)
    {
        var x = boxes.GetLength(0);
        var y = boxes.GetLength(1);
        var z = boxes.GetLength(2);

        float ymin = 0, xmin = 0, ymax = 0, xmax = 0;
        var results = new List<BoxOutline>();

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (scores[i, j] < minScore) continue;

                for (int k = 0; k < z; k++)
                {
                    var box = boxes[i, j, k];
                    switch (k)
                    {
                        case 0:
                            ymin = box;
                            break;
                        case 1:
                            xmin = box;
                            break;
                        case 2:
                            ymax = box;
                            break;
                        case 3:
                            xmax = box;
                            break;
                    }
                }

                int value = Convert.ToInt32(classes[i, j]);
                var label = this.labels[value];
                var boxOutline = new BoxOutline
                {
                    YMin = ymin,
                    XMin = xmin,
                    YMax = ymax,
                    XMax = xmax,
                    Label = label,
                    Score = scores[i, j],
                };

                results.Add(boxOutline);
            }
        }

        return results;
    }
}