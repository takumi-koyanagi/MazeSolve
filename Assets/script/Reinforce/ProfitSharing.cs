using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProfitSharing
{
    double gamma = 0;
    double epsilon = 0.01f;
    double exP = 0.8f;
    double tau = 0.4f;

    const int EPSGREEDY = 11;
    const int SOFTMAX = 22;
    int actionSelector = EPSGREEDY;

    private double[][] Omega;
    private int[] episodeS;
    private int[] episodeA;
    int T = 0;
    int episodeT = 1;
    double L;


    public ProfitSharing(double AttenuationRate, int episodeLimit, int[] s, int[] a)
    {
        gamma = AttenuationRate;
        L = (double)(a.Length - 1);
        if (gamma > 1 / (L + 1)) gamma = 1 / (L + 1);

        episodeS = new int[episodeLimit];
        episodeA = new int[episodeLimit];

        Omega = new double[s.Length][];
        for (int i = 0; i < s.Length; i++) Omega[i] = new double[a.Length];
        for (int i = 0; i < s.Length; i++)
        {
            for (int j = 0; j < a.Length; j++)
            {
                Omega[i][j] = 1.0f;
            }
        }
    }

    public void EpisodeUpdate(int s, int a)
    {
        if (episodeS.Length <= T) Reset();
        episodeS[T] = s;
        episodeA[T] = a;
        T++;
    }

    public void ValueUpdate(double r)
    {
        if (r > 0.1f || r < -0.1f)
        {
            for (int t = 0; t < T; t++)
            {
                Omega[episodeS[T - 1 - t]][episodeA[T - 1 - t]]
                    = Omega[episodeS[T - 1 - t]][episodeA[T - 1 - t]] + f(r, t);
            }

            T = 0;

            if (r > 0) {
                //ゴールしたエピソードおきに（報酬をもらったときに）Epsilonを減少させる．
                episodeT += 1;
            }

            EpsilonSet(exP / (double)(episodeT));
            Debug.Log(exP / (double)(episodeT));
        }
    }

    public double f(double r , int t)
    {
        return r * Math.Pow(gamma, T - 1 - t);
    }

    public int SelectAction(int s)
    {
        int tardex = 0;

        switch (actionSelector)
        {
            case EPSGREEDY:
                tardex = epsilon_greedy_selection(Omega[s]);
                break;
            case SOFTMAX:
                tardex = softmax_selection(Omega[s]);
                break;
        }

        return tardex;
    }

    public void Reset()
    {
        for (int t = 0; t < T; t++)
        {
            episodeS[t] = 0;
            episodeA[t] = 0;
        }

        T = 0;
    }

    public int epsilon_greedy_selection(double[] values)
    {
        double[] p = { epsilon, 1.0f - epsilon };

        if (selection_from_dispersion(p) == 0)
        {
            return random_selection(values);
        }
        else
        {
            return max_selection(values);
        }
    }

    public int softmax_selection(double[] values)
    {
        int values_num = values.Length;
        double[] probability = new double[values_num];
        double probabilitySum = 0.0f;

        for (int i = 0; i < values_num; i++)
        {
            probability[i] = Math.Pow(Math.E, values[i] / tau);
            probabilitySum += probability[i];
        }

        for (int i = 0; i < values_num; i++)
        {
            probability[i] = probability[i] / probabilitySum;
        }

        return selection_from_dispersion(probability);
    }

    //与えられた確率分布の通りにインデックスを選択する
    public int selection_from_dispersion(double[] probability)
    {
        int values_num = probability.Length;
        int tardex = 0;
        System.Random r = new System.Random();
        double rand = r.NextDouble();
        double[] pSum = new double[values_num];

        pSum[0] = probability[0];
        for (int i = 1; i < values_num; i++)
        {
            pSum[i] = probability[i] + pSum[i - 1];
        }

        for (int i = 0; i < values_num; i++)
        {
            if (rand <= pSum[i])
            {
                tardex = i;
                break;
            }
        }

        return tardex;
    }

    public int random_selection(double[] values)
    {
        int values_num = values.Length;
        System.Random r = new System.Random();
        double rand = r.NextDouble();
        int tardex = (int)((rand * 1000) % values_num);

        return tardex;
    }

    public int max_selection(double[] values)
    {
        int values_num = values.Length;
        int maxdex = 0;

        for (int i = 1; i < values_num; i++)
        {
            if (values[i] > values[maxdex]) maxdex = i;
        }

        return maxdex;
    }

    public void EpsilonSet(double eps)
    {
        if (eps < 0) eps = 0.0f;
        if (eps > 1) eps = 1.0f;
        epsilon = eps;
    }
    
    public void GammaSet(double gam)
    {
        gamma = gam;
    }
    public void TauSet(double t)
    {
        tau = t;
    }
}
