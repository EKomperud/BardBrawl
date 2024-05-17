using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory
{
    protected float t;
    protected float y;

    public virtual float SetTime(float time)
    {
        t = time;
        y = time;
        return t;
    }

    public virtual float IncrementTime(float increment)
    {
        t += increment;
        y = t;
        return t;
    }

    protected virtual float Function()
    {
        return y;
    }

    public virtual float CheckFunctionInverse(float y)
    {
        return t;
    }
}

public class OneWayParabola : Trajectory
{
    protected float xCoefficient;         // How wide the Parabola is (bigger numbers shrink, fractions widen)
    protected float xShift;               // Shifts the Parabola on the X-axis
    protected float yShift;               // Shifts the Parabola on the Y-axis
    protected float negative;             // Flips the Parabola upside-down when negative;
    protected float yLimit;
    protected bool brokeThroughFloor;
    protected bool reachedLimit;

    public OneWayParabola(float xCoefficient, float xShift, float yShift, bool negative, float yLimit)
    {
        t = 0f;
        y = 0f;
        brokeThroughFloor = false;

        this.xCoefficient = xCoefficient;
        this.xShift = -xShift;
        this.yShift = yShift;
        this.negative = negative ? -1 : 1;   
        this.yLimit = yLimit;
    }

    public  void Retune(float xCoefficient, float xShift, float yShift, bool negative, float yLimit)
    {
        this.xCoefficient = xCoefficient;
        this.xShift = -xShift;
        this.yShift = yShift;
        this.negative = negative ? -1 : 1;
        this.yLimit = yLimit;
    }

    public override float SetTime(float time)
    {
        t = time;
        return Function();
    }

    public override float IncrementTime(float increment)
    {
        t += increment;
        return Function();
    }

    public bool CheckAtLimit()
    {
        return reachedLimit;
    }

    protected override float Function()
    {
        if (!reachedLimit)
        {
            float r = negative * Mathf.Pow(((xCoefficient * t) + xShift), 2) + yShift;
            if (r > yLimit)
            {
                brokeThroughFloor = true;
            }
            if (r < yLimit && brokeThroughFloor)
            {
                r = yLimit;
                reachedLimit = true;
            }
            return r;
        }
        else
        {
            return yLimit;
        }
    }
}