using UnityEngine;
using System.Collections;
using System;

public class Heap<T> where T : IHeapItem<T> {
	
    T[] items;
    int currentItemCount;
	
    public Heap(int maxHeapSize) {
        items = new T[maxHeapSize];
    }
	
    public void Add(T item) {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    public T RemoveFirst() {
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    public void UpdateItem(T item) {
        SortUp(item);
    }

    public int Count {
        get {
            return currentItemCount;
        }
    }

    public bool Contains(T item) {
        return Equals(items[item.HeapIndex], item);
    }

    void SortDown(T item) {
        while (true) {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < currentItemCount) {
                swapIndex = childIndexLeft;

                if (childIndexRight < currentItemCount) {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0) {
                        swapIndex = childIndexRight;
                    }
                }

                if (item.CompareTo(items[swapIndex]) < 0) {
                    Swap (item,items[swapIndex]);
                }
                else {
                    return;
                }

            }
            else {
                return;
            }

        }
    }
	
    void SortUp(T item) {
        int parentIndex = (item.HeapIndex-1)/2;
		
        while (true) {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0) {
                Swap (item,parentItem);
            }
            else {
                break;
            }

            parentIndex = (item.HeapIndex-1)/2;
        }
    }
	
    void Swap(T itemA, T itemB) {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

public interface IHeapItem<T> : IComparable<T> {
    int HeapIndex {
        get;
        set;
    }
}


// public class Heap
// {
//     public int size;
//     public Node[] heap;
//     public int current;
//     public Heap(int size)
//     {
//         this.size = size;
//         heap = new Node[size + 1];
//         current = 0;
//     }
//     public void BuildHeap(Node[] arr)
//     {
//         if (arr.Length > 0)
//             foreach (Node node in arr)
//                 Insert(node);
//     }
//     public void Insert(Node x)
//     {
//         if (current == 0)
//         {
//             heap[current + 1] = x;
//             x.HeapIndex = current + 1;
//             current = 2;
//         }
//         else {
//             x.HeapIndex = current;
//             heap[current++] = x;
//             BubbleUp();
//         }
//     }
//     public void BubbleUp()
//     {
//         int pos = current - 1;
//         while (pos > 0 && (heap[pos / 2].CompareTo( heap[pos]) < 0))
//         {
//             Node y = heap[pos];
//             heap[pos] = heap[pos / 2];
//             heap[pos / 2] = y;
//             pos = pos / 2;
//         }
//     }
//     public Node ExtractMin()
//     {
//         Node min = heap[1];
//         heap[1] = heap[current - 1];
//         // heap[current - 1] = null;
//         current--;
//         SinkDown(1);
//         return min;
//     }
//
//     public void SinkDown(int k)
//     {
//         Node a = heap[k];
//         int smallest = k;
//         // left child
//         if (2 * k < current && heap[smallest].CompareTo(heap[2 * k]) > 1)
//         {
//             smallest = 2 * k;
//         }
//         // right child
//         if (2 * k + 1 < current && heap[smallest].CompareTo(heap[2 * k + 1]) > 1)
//         {
//             smallest = 2 * k + 1;
//         }
//         if (smallest != k)
//         {
//             Swap(k, smallest);
//             SinkDown(smallest);
//         }
//
//     }
//     public void Swap(int a, int b)
//     {
//         Node temp = heap[a];
//         heap[a] = heap[b];
//         heap[b] = temp;
//     }
//
//     public bool Contains(Node node)
//     {
//         return Equals(heap[node.HeapIndex], node);
//     }
// }
