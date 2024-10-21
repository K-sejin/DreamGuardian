using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBox
{
    private static readonly List<(string item, double probability)> items = new List<(string, double)>()
    {
        ("Wood", 40),
        ("Sand", 20),
        ("Steel", 20),
        ("WoodenPlank", 5),
        ("Clay", 5),
        ("ForgedSteel", 2.5),
        ("Pedestal_Basic", 1),
        ("Body_Basic", 0.5),
        ("Gun_Basic", 0.7),
        ("Gun_Double", 0.3),
        ("Pedestal_Enforce", 0.5),
        ("Body_Enforce", 0.25),
        ("Gun_Fire", 0.15),
        ("Gun_Ice", 0.15),
        ("Barrier", 0.15),
        ("Jayden", 1.25),
        ("Brody", 1.25),
        ("Casey", 1.25),
        ("Laura", 0.05)
    };

    public static string GetRandomItem()
    {
        // UnityEngine.Random.Range�� ����Ͽ� 0~100 ������ ������ ���� ����
        float roll = Random.Range(0f, 100f);
        double cumulativeProbability = 0.0;

        foreach (var item in items)
        {
            cumulativeProbability += item.probability;
            if (roll < cumulativeProbability)
            {
                return item.item;
            }
        }

        // Ȯ���� �߸� ������ ��츦 ����� �⺻��
        return "NULL";
    }
}