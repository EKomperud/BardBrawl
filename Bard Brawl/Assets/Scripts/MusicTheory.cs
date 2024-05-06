using System;
using System.Collections;
using System.Collections.Generic;

public static class MusicTheory
{
    private static int[] valuesMajor = new int[12] { 60, 67, 62, 69, 64, 71, 66, 61, 68, 63, 70, 65 };
    private static int[] valuesMinor = new int[12] { 69, 64, 71, 66, 61, 68, 63, 70, 65, 60, 67, 62 };
    private static int[] valuesTriads = new int[6] { 43, 34, 33, 44, 52, 25 };

    public enum Majors
    {
        C,
        G,
        D,
        A,
        E,
        B,
        F_Sharp,
        D_Flat,
        A_Flat,
        E_Flat,
        B_Flat,
        F
    }

    public enum Minors
    {
        a,
        e,
        b,
        f_Sharp,
        c_Sharp,
        g_Sharp,
        d_Sharp,
        b_Flat,
        f,
        c,
        g,
        d
    }
    public enum Triads
    {
        Major,
        Minor,
        Dim,
        Aug,
        Sus2,
        Sus4,
    }

    public static int[] GetTriad (Majors key, Triads triad, int shift)
    {
        int[] t = new int[3];
        int p = valuesMajor[(int)key];
        t[0] = p;
        int two;
        int three;
        two = Math.DivRem(valuesTriads[(int)triad], 10, out three);
        p = t[1] = p + two;
        t[2] = p + three;

        for (int i = 0; i < 3; i++)
        {
            t[i] += (12 * shift);
            if (t[i] < 0 || t[i] + 12 > 127)
            {
                throw new ArgumentOutOfRangeException("shift", "shift caused chord to be beyond maximum note values");
            }
        }

        return t;
    }
}
