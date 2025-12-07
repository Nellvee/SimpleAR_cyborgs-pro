using System;
using System.Collections;
using System.Threading.Tasks;
namespace Project.Extensions
{
    public static class TaskExstensions
    {
        /// <summary>
        /// Wait <see cref="Task"/> while not <see cref="Task.IsCompleted"/>
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static IEnumerator AsIEnumerator(this Task task)
        {
            //Wait for complete
            while (!task.IsCompleted)
            {
                yield return null;
            }
            ///throw exception if task was faulted
            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }

        /// <summary>
        /// Wait <see cref="Task"/> while not <see cref="Task.IsCompleted"/>
        /// <para>Use <see cref="Action{C}"/> to get result from IEnumerator</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static IEnumerator AsIEnumerator<T>(this Task<T> task, Action<T> result)
        {
            yield return task.AsIEnumerator();
            /*if (task.IsFaulted)
            {
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
            }*/
            result?.Invoke(task.Result);
        }

    }
}