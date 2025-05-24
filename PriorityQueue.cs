using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AlmoustCourseWork
{
    public class PriorityQueue
    {
        private readonly List<Node> heap = new List<Node>();

        public int Count => heap.Count;

        //Додає вузол у чергу на основі node.F
        public void Enqueue(Node node)
        {
            heap.Add(node);
            HeapifyUp(heap.Count - 1);
        }

        //Видаляє та повертає вузол з мінімальним F
        public Node Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("PriorityQueue is empty.");

            // Корінь купи — найменший F
            Node min = heap[0];

            // Переміщаємо останній елемент на корінь і просіюємо вниз
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            if (heap.Count > 0)
                HeapifyDown(0);

            return min;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (heap[index].F < heap[parent].F)
                {
                    Swap(index, parent);
                    index = parent;
                }
                else
                    break;
            }
        }

        private void HeapifyDown(int index)
        {
            int last = heap.Count - 1;
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;

                if (left <= last && heap[left].F < heap[smallest].F)
                    smallest = left;
                if (right <= last && heap[right].F < heap[smallest].F)
                    smallest = right;

                if (smallest != index)
                {
                    Swap(index, smallest);
                    index = smallest;
                }
                else
                    break;
            }
        }

        private void Swap(int i, int j)
        {
            var temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }
    }
}
