using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Memento<T>
{
    public List<T> Items;

    public Memento(List<T> items)
    {
        Items = new List<T>(items);
    }
}
