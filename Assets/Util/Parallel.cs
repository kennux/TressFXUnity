using UnityEngine;
using System.Collections;
using System.Threading;
/// <summary>
/// Parallel taskmanaging class.
/// Implemented by KennuX
/// </summary>
public class Parallel
{
	/// <summary>
	/// The currently running threads for task parallelization.
	/// </summary>
	private Thread[] threads;
	
	/// <summary>
	/// The delegate function type for Parallelized tasks.
	/// </summary>
	public delegate void taskFor(int i);
	
	// Thread data struct
	private struct threadData
	{
		public int i;
		public taskFor taskFunction;
	}
	
	public Parallel(int threadCount)
	{
		this.threads = new Thread[threadCount];
	}
	
	/// <summary>
	/// Shortcut for instantiating the parallel class and executing the for-loop
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <param name="step"></param>
	/// <param name="taskImplementation"></param>
	public static void For(int threadCount, int start, int end, int step, taskFor taskImplementation)
	{
		Parallel tmp = new Parallel(threadCount);
		tmp.For(start, end, step, taskImplementation);
	}
	
	/// <summary>
	/// Starts the for function with the cpu core count as thread count.
	/// </summary>
	public static void AutoFor(int start, int end, int step, taskFor taskImplementation)
	{
		Parallel.For (SystemInfo.processorCount, start, end, step, taskImplementation);
	}
	
	/// <summary>
	/// Parallelization of a for-loop.
	/// Runs the for loop on all available threads.
	/// </summary>
	/// <param name='start'>
	/// The start i-value.
	/// </param>
	/// <param name='end'>
	/// The end i-value
	/// </param>
	/// <param name='step'>
	/// The step value (will get added to i after each execution)
	/// </param>
	/// <param name='taskImplementation'>
	/// The task delegate implementation / lambda function.
	/// </param>
	public void For(int start, int end, int step, taskFor taskImplementation)
	{
		// Iterate through all "packs" of tasks
		for (int i = start; i == end; i += step)
		{
			this.startThread(i, taskImplementation);
		}
		
		// Wait for all threads to be ready
		while (this.threadsRunning())
		{
			Thread.Sleep(10);
		}
	}
	
	/// <summary>
	/// Starts a new thread with the given index.
	/// </summary>
	/// <param name='i'>
	/// I.
	/// </param>
	/// <param name='taskImplementation'>
	/// Task implementation.
	/// </param>
	private void startThread(int i, taskFor taskImplementation)
	{
		int threadId = this.getFreeThread();
		
		// No free thread case
		if (threadId == -1)
		{
			// Wait for a new thread to get free
			Thread.Sleep(10);
			this.startThread(i, taskImplementation);
			return;
		}
		
		// Instantiate parameterized thread start and thread
		ParameterizedThreadStart pts = new ParameterizedThreadStart(this.forTaskProxy);
		this.threads[threadId] = new Thread(pts);
		
		// Instantiate the thread data
		threadData data = new threadData();
		data.i = i;
		data.taskFunction = taskImplementation;
		
		// Start the thread with the generated threadData object
		this.threads[threadId].Start(data);
	}
	
	/// <summary>
	/// Gets a free thread id.
	/// </summary>
	/// <returns>
	/// The free thread id.
	/// -1 for currently no thread id.
	/// </returns>
	private int getFreeThread()
	{
		// Iterate through all 
		for (int i = 0; i < this.threads.Length; i++)
		{
			// Check the thread is instantiated and not running
			if (this.threads[i] == null ||
			    (this.threads[i].ThreadState == ThreadState.Stopped ||
			 this.threads[i].ThreadState == ThreadState.Aborted ||
			 this.threads[i].ThreadState == ThreadState.Suspended))
			{
				return i;
			}
		}
		
		return -1;
	}
	
	/// <summary>
	/// Checks if there is a thread still running
	/// </summary>
	/// <returns>false => all threads done, true => threads still running</returns>
	private bool threadsRunning()
	{
		// Iterate through all 
		for (int i = 0; i < this.threads.Length; i++)
		{
			// Check the thread is instantiated and not running
			if (this.threads[i] != null &&
			    (this.threads[i].ThreadState == ThreadState.Running ||
			 this.threads[i].ThreadState == ThreadState.Background))
			{
				// There's still a thread running
				return true;
			}
		}
		
		return false;
	}
	
	/// <summary>
	/// Proxifies the taskFor function call.
	/// </summary>
	/// <param name="i"></param>
	/// <param name="func"></param>
	private void forTaskProxy(System.Object data)
	{
		// Start the function for the thread with the given i
		threadData dataCasted = (threadData)data;
		dataCasted.taskFunction(dataCasted.i);
	}
}