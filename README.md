See a more update edition of this document [here](https://github.com/duongphuhiep/ToolsPack.Net/wiki/Async-Await-Task)

# [Asynchronous Patterns]

* APM = Asynchronous Programming Model  = IAsyncResult = Begin / End (not recommended)
* EAP = Event-based Asynchronous Pattern = Async suffix + events, event handler + EventArg (not recommended)
* TAP = Task-based Asynchronous Pattern = async / await (recommended)

TAP method return Task or `Task<TResult>` and don't accept `out` and `ref` parameters. It might [support Cancellation / Progression]

# [Task.Run vs Task.Factory.StartNew]

`Task.Run` (since .NET 4.5) = sugar shortcut for `Task.Factory.StartNew`

`Task.Factory.StartNew` oftens return `Task<Task>` you will have to unwrap() it..
```c#
var t = Task.Factory.StartNew(async ()=>{
	await Task.Delay(300);
}); //t is Task<Task<VoidTaskResult>>
var tt = t.Unwrap(); //tt is Task<VoidTaskResult>
tt.Wait(); //t.Wait() will return immediately so we must to wait the unwrap task
```

# [IO-Bounds and CPU-bounds]

[Source](https://channel9.msdn.com/Series/Three-Essential-Tips-for-Async/Tip-2-Distinguish-CPU-Bound-work-from-IO-bound-work)

- If the work you have is I/O-bound, use `async` and `await`. You should not use the TPL (`Task.Run`or `Parallel.For`..)
- If the work you have is CPU-bound and you care about responsiveness, use `async` and `await` but spawn the work off on another thread with `Task.Run`

You will mostly see I/O-bounds code. The CPU-bounds code is quite rare, it is codes with big for-loop, heavy computational eg. regular expression, linq over object..

Example: I/O-bounds code Load data from 100 databases:

**Good code**: attack all the database at once with asyn / await
```C#
public async Task<List<House>> LoadHousesAsync() {
	var tasks = new List<Task<House>>();
	foreach (var db in DatabaseList) {
		Task<House> t = db.LoadHouseAsync();
		tasks.Add(t);
	}
	House[] resu = await Task.WhenAll(tasks);
	return resu.ToList();
}
```

**Bad code**: use TPL to spawn new threads which wait for I/O
```C#
public async List<House> LoadHousesInParalle() {
	var resu = new BlockingCollection<House>();
	Parallel.ForEach (DatabaseList, db => {
		House house = db.LoadHouse();
		resu.Add(house);
	});
	return resu.ToList();
}
```

# [Void-Returning Task is only for event handler]

* `async void f()` is a *void-returning* task (vrt)
* `async Task f()` is a *task-returning* task 

* A *void-returning* task (vrt) is a "fire and forget" mechanism
  * The caller is unable to know when the vrt will finish:
  `await vrt` won't wait until the `vrt` finish, it only wait until the first `await` inside the `vrt` body. 
  * The caller is unable to catch exceptions inside of the vrt.
* Use vrt only for top-level event handlers (or if you have no choices)
* Whenever you see an async lambda, it could be *void-returning* or *task-returning* => **you will have to verify it every single time that you see it**

Example 1:
```c#
Action     f1 = async () => { Console.WriteLine("f1 begin"); await Task.Delay(300); Console.WriteLine("f1 end"); };
Func<Task> f2 = async () => { Console.WriteLine("f2 begin"); await Task.Delay(300); Console.WriteLine("f2 end"); };

var t1 = Task.Run(f1); //t1 is Task<void> (t1 is a void-returning task)
t1.Wait(); //it won't wait, but return immediately (Run and Forget)
Console.WriteLine("t1 wait exit");

var t2 = Task.Run(f2); //t2 is Task<VoidTaskResult> (t2 is a task-returning task)
t2.Wait(); //it will wait
Console.WriteLine("t2 wait exit");

Task.Delay(1000).Wait();
```

result

```
f1 begin
t1 wait exit
f2 begin
f1 end
f2 end
t2 wait exit
```

in case overloading ambigues: `Task.Run(async()=>{..})` => it will consider the parameter as *task-returning* => no worry!


Example 2:
```c#

//Task RunAsync(Priority priority, DispatchedHandler h);
//delegate void DispatchedHandler();

try {
	await dispatcher.RunAsync(Priority.Normal, async() => {
		await LoadAsync();
		throw new Exception();
	})
}
catch (Exception) {
	...
}
```
There is a async lambda in this code => we must to check if it is "async void" or "async task"..
The async lambda is a `DispatchedHandler` which returns `void` => so it is a *void-returning* task => so the `await` and `try / catch` in the caller is completly **useless**. It won't wait nor catching any exception..

# [Anti-pattern]

## 1. Avoid fake async codes: All methods suffixed with *Async* must to be marked with the `async` keyword. Example:

**Good code**
```C#
static async Task ComputeAsync() {
	..await somewhere
}
```
=> if the body won't spawn any new thread => the code is truely async

**BAD code**
```C#
static Task ComputeAsync() {
	return Task.Run(() => { //spaw a new thread
		Compute();
	});
}
```
=> the code look like async, but it is fake, it use up CPU and ThreadPool => it is beter to keep only the sync version of `Compute()` and let the callers decide whether they will run it in other thread or not..

## 2. Avoid fake sync codes

**VERY BAD code**
```C#
static Task Compute() {
	var t = ComputeAsync();
	t.Wait(); //freeze the UI Thread (SynchronizationContext) 
}

static async Task ComputeAsync() {
	await
	return something; //wait for the UI Thread
}
```

it look like sync codes, but it isn't, and it might cause dead-lock:
- the UI-Thread is freezed because the ComputeAsync is not finished
- the ComputeAsync is not finished because the UI-Thread is freezed

To avoid deadlock:
- In your “library” async methods, use `ConfigureAwait(false)` wherever possible.
- Don’t block on Tasks; use async all the way down.



[Asynchronous Patterns]: https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/index
[support Cancellation / Progression]: https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap
[Task.Run vs Task.Factory.StartNew]: https://blogs.msdn.microsoft.com/pfxteam/2011/10/24/task-run-vs-task-factory-startnew/
[IO-Bounds and CPU-bounds]: https://docs.microsoft.com/en-us/dotnet/csharp/async
[Void-Returning Task is only for event handler]: https://channel9.msdn.com/Series/Three-Essential-Tips-for-Async/Tip-1-Async-void-is-for-top-level-event-handlers-only
[Anti-pattern]: https://channel9.msdn.com/Series/Three-Essential-Tips-for-Async/Async-Library-Methods-Shouldn-t-Lie