using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace Assets
{
    internal static class QueueExtensions
    {
        public static void EnqueuAndPush<T>(this Queue<T> values ,T Item, int maxItemCount)
        {
            if(values.Count >= maxItemCount) { values.Dequeue(); }
            values.Enqueue(Item);
        }
    }
}
