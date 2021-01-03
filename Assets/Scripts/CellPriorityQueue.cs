using System.Collections.Generic;

public class CellPriorityQueue
{
    List<Cell> list = new List<Cell>();
    int minimum = int.MaxValue;

    int count = 0;
    public int Count {
        get { return count; }
    }

    public void Enqueue(Cell cell)
    {
        count += 1;
        int priority = cell.SearchPriority;
        if (priority < minimum) {
            minimum = priority;
        }
        while (priority >= list.Count) {
            list.Add(null);
        }
        cell.NextWithSamePriority = list[priority];
        list[priority] = cell;
    }

    public Cell Dequeue()
    {
        count -= 1;
        for (; minimum < list.Count; minimum++) {
            Cell cell = list[minimum];
            if (cell != null) {
                list[minimum] = cell.NextWithSamePriority;
                return cell;
            }
        }
        return null;
    }

    public void Change(Cell cell, int oldPriority)
    {
        Cell current = list[oldPriority];
        Cell next = current.NextWithSamePriority;
        if (current == cell) {
            list[oldPriority] = next;
        }
        else {
            while (next != cell) {
                current = next;
                next = current.NextWithSamePriority;
            }
            current.NextWithSamePriority = cell.NextWithSamePriority;
        }
        Enqueue(cell);
        count -= 1;
    }

    public void Clear()
    {
        list.Clear();
        count = 0;
        minimum = int.MaxValue;
    }
}
