using UnityEngine;

[System.Serializable]
public class LineSegment
{
    public Vector3 StartPoint  /*{ get; set; }*/ = Vector3.zero;
    public Vector3 EndPoint    /*{ get; set; }*/ = Vector3.zero;
    public float Amplitude     /*{ get; set; }*/ = 0.0f;
    public int SubDivisions    /*{ get; set; }*/ = 0;
    public float Thickness     /*{ get; set; }*/ = 0.1f;
    public float FillPercentage/*{ get; set; }*/ = 0.0f;

    public LineSegment(Vector3 start, Vector3 end, float amplitude, int subDivisions, float thickness, float fillPercentage)
    {
        this.StartPoint = start;
        this.EndPoint = end;
        this.Amplitude = amplitude;
        this.SubDivisions = subDivisions;
        this.Thickness = thickness;
        this.FillPercentage = fillPercentage;
    }

    public LineSegment(LineSegment other)
    {
        this.StartPoint = other.StartPoint;
        this.EndPoint = other.EndPoint;
        this.Amplitude = other.Amplitude;
        this.SubDivisions = other.SubDivisions;
        this.Thickness = other.Thickness;
        this.FillPercentage = other.FillPercentage;
    }

}