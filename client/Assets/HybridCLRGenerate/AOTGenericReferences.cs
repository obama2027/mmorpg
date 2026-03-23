using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"Google.Protobuf.dll",
		"System.Core.dll",
		"UnityEngine.AssetBundleModule.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Google.Protobuf.IDeepCloneable<object>
	// Google.Protobuf.IMessage<object>
	// Google.Protobuf.MessageParser.<>c__DisplayClass2_0<object>
	// Google.Protobuf.MessageParser<object>
	// System.Action<SocketConnection.ReceivedPacket>
	// System.Action<SocketConnection.SocketEvent>
	// System.Action<float>
	// System.Action<int,int>
	// System.Action<int,object,int>
	// System.Action<int,object>
	// System.Action<object,object>
	// System.Action<object>
	// System.Buffers.ArrayPool<byte>
	// System.Buffers.ConfigurableArrayPool.Bucket<byte>
	// System.Buffers.ConfigurableArrayPool<byte>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool.LockedStack<byte>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool.PerCoreLockedStacks<byte>
	// System.Buffers.TlsOverPerCoreLockedStacksArrayPool<byte>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<SocketConnection.ReceivedPacket>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<SocketConnection.SendPacket>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<SocketConnection.SocketEvent>
	// System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<object>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<SocketConnection.ReceivedPacket>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<SocketConnection.SendPacket>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<SocketConnection.SocketEvent>
	// System.Collections.Concurrent.ConcurrentQueue.Segment<object>
	// System.Collections.Concurrent.ConcurrentQueue<SocketConnection.ReceivedPacket>
	// System.Collections.Concurrent.ConcurrentQueue<SocketConnection.SendPacket>
	// System.Collections.Concurrent.ConcurrentQueue<SocketConnection.SocketEvent>
	// System.Collections.Concurrent.ConcurrentQueue<object>
	// System.Collections.Generic.ArraySortHelper<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.ArraySortHelper<SocketConnection.SocketEvent>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.Comparer<SocketConnection.SocketEvent>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,AssetAddress>
	// System.Collections.Generic.Dictionary.Enumerator<object,SceneAddress>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.Enumerator<ushort,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,AssetAddress>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,SceneAddress>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<ushort,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,AssetAddress>
	// System.Collections.Generic.Dictionary.KeyCollection<object,SceneAddress>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<ushort,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,AssetAddress>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,SceneAddress>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<ushort,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,AssetAddress>
	// System.Collections.Generic.Dictionary.ValueCollection<object,SceneAddress>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<ushort,object>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,AssetAddress>
	// System.Collections.Generic.Dictionary<object,SceneAddress>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.Dictionary<ushort,object>
	// System.Collections.Generic.EqualityComparer<AssetAddress>
	// System.Collections.Generic.EqualityComparer<SceneAddress>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.EqualityComparer<ushort>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.ICollection<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.ICollection<SocketConnection.SendPacket>
	// System.Collections.Generic.ICollection<SocketConnection.SocketEvent>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,AssetAddress>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,SceneAddress>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<ushort,object>>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.IComparer<SocketConnection.SocketEvent>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.IEnumerable<SocketConnection.SendPacket>
	// System.Collections.Generic.IEnumerable<SocketConnection.SocketEvent>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,AssetAddress>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,SceneAddress>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<ushort,object>>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.IEnumerator<SocketConnection.SendPacket>
	// System.Collections.Generic.IEnumerator<SocketConnection.SocketEvent>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,AssetAddress>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,SceneAddress>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<ushort,object>>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IEqualityComparer<ushort>
	// System.Collections.Generic.IList<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.IList<SocketConnection.SocketEvent>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<object,AssetAddress>
	// System.Collections.Generic.KeyValuePair<object,SceneAddress>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.KeyValuePair<ushort,object>
	// System.Collections.Generic.List.Enumerator<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.List.Enumerator<SocketConnection.SocketEvent>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.List<SocketConnection.SocketEvent>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<SocketConnection.ReceivedPacket>
	// System.Collections.Generic.ObjectComparer<SocketConnection.SocketEvent>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<AssetAddress>
	// System.Collections.Generic.ObjectEqualityComparer<SceneAddress>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<ushort>
	// System.Collections.ObjectModel.ReadOnlyCollection<SocketConnection.ReceivedPacket>
	// System.Collections.ObjectModel.ReadOnlyCollection<SocketConnection.SocketEvent>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<SocketConnection.ReceivedPacket>
	// System.Comparison<SocketConnection.SocketEvent>
	// System.Comparison<object>
	// System.Func<System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,byte>
	// System.Func<object,object>
	// System.Func<object>
	// System.IEquatable<object>
	// System.Predicate<SocketConnection.ReceivedPacket>
	// System.Predicate<SocketConnection.SocketEvent>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory<object>
	// UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene>
	// }}

	public void RefMethods()
	{
		// object System.Activator.CreateInstance<object>()
		// byte[] System.Array.Empty<byte>()
		// object[] System.Array.Empty<object>()
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetBundleManager.<EnsureManifestLoadedAsync>d__28>(System.Runtime.CompilerServices.TaskAwaiter&,AssetBundleManager.<EnsureManifestLoadedAsync>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetBundleManager.<InitializeAsync>d__15>(System.Runtime.CompilerServices.TaskAwaiter&,AssetBundleManager.<InitializeAsync>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,BundleSceneLoader.<LoadSceneAsync>d__7>(System.Runtime.CompilerServices.TaskAwaiter&,BundleSceneLoader.<LoadSceneAsync>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,BundleSceneLoader.<UnloadSceneAsync>d__8>(System.Runtime.CompilerServices.TaskAwaiter&,BundleSceneLoader.<UnloadSceneAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,GameAssetService.<ReleaseGroupAndCollectAsync>d__33>(System.Runtime.CompilerServices.TaskAwaiter&,GameAssetService.<ReleaseGroupAndCollectAsync>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,NetworkDownloader.<DownloadToStreamAsync>d__3>(System.Runtime.CompilerServices.TaskAwaiter&,NetworkDownloader.<DownloadToStreamAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<ClearAllAsync>d__28>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<ClearAllAsync>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<EnsureBundleReadyAsync>d__32>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<EnsureBundleReadyAsync>d__32&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<EnsurePackageReadyAsync>d__33>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<EnsurePackageReadyAsync>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadAssetAsync>d__20<object>>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadAssetAsync>d__20<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadAssetsAsync>d__21<object>>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadAssetsAsync>d__21<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadBundleAsync>d__18>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadBundleAsync>d__18&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadBundlesAsync>d__19>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadBundlesAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadGroupAsync>d__22>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadGroupAsync>d__22&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AssetBundleManager.<LoadManifestAsync>d__29>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AssetBundleManager.<LoadManifestAsync>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AssetBundleManager.<PreloadBundleAsync>d__19>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AssetBundleManager.<PreloadBundleAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,NetworkDownloader.<DownloadToStreamAsync>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,NetworkDownloader.<DownloadToStreamAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,PreloadService.<PreloadAssetAsync>d__20<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,PreloadService.<PreloadAssetAsync>d__20<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,AssetBundleManager.<LoadManifestAsync>d__29>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,AssetBundleManager.<LoadManifestAsync>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,AssetBundleManager.<UnloadUnusedAssetsAsync>d__23>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,AssetBundleManager.<UnloadUnusedAssetsAsync>d__23&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,BundleSceneLoader.<LoadSceneAsync>d__7>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,BundleSceneLoader.<LoadSceneAsync>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,BundleSceneLoader.<UnloadSceneAsync>d__8>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,BundleSceneLoader.<UnloadSceneAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,NetworkDownloader.<DownloadAttemptToFileAsync>d__5>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,NetworkDownloader.<DownloadAttemptToFileAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetBundleManager.<EnsureManifestLoadedAsync>d__28>(System.Runtime.CompilerServices.TaskAwaiter&,AssetBundleManager.<EnsureManifestLoadedAsync>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetBundleManager.<InitializeAsync>d__15>(System.Runtime.CompilerServices.TaskAwaiter&,AssetBundleManager.<InitializeAsync>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,BundleSceneLoader.<LoadSceneAsync>d__7>(System.Runtime.CompilerServices.TaskAwaiter&,BundleSceneLoader.<LoadSceneAsync>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,BundleSceneLoader.<UnloadSceneAsync>d__8>(System.Runtime.CompilerServices.TaskAwaiter&,BundleSceneLoader.<UnloadSceneAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,GameAssetService.<ReleaseGroupAndCollectAsync>d__33>(System.Runtime.CompilerServices.TaskAwaiter&,GameAssetService.<ReleaseGroupAndCollectAsync>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,NetworkDownloader.<DownloadToStreamAsync>d__3>(System.Runtime.CompilerServices.TaskAwaiter&,NetworkDownloader.<DownloadToStreamAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<ClearAllAsync>d__28>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<ClearAllAsync>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<EnsureBundleReadyAsync>d__32>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<EnsureBundleReadyAsync>d__32&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<EnsurePackageReadyAsync>d__33>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<EnsurePackageReadyAsync>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadAssetAsync>d__20<object>>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadAssetAsync>d__20<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadAssetsAsync>d__21<object>>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadAssetsAsync>d__21<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadBundleAsync>d__18>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadBundleAsync>d__18&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadBundlesAsync>d__19>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadBundlesAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,PreloadService.<PreloadGroupAsync>d__22>(System.Runtime.CompilerServices.TaskAwaiter&,PreloadService.<PreloadGroupAsync>d__22&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AssetBundleManager.<LoadManifestAsync>d__29>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AssetBundleManager.<LoadManifestAsync>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AssetBundleManager.<PreloadBundleAsync>d__19>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AssetBundleManager.<PreloadBundleAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,NetworkDownloader.<DownloadToStreamAsync>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,NetworkDownloader.<DownloadToStreamAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,PreloadService.<PreloadAssetAsync>d__20<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,PreloadService.<PreloadAssetAsync>d__20<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,AssetBundleManager.<LoadManifestAsync>d__29>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,AssetBundleManager.<LoadManifestAsync>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,AssetBundleManager.<UnloadUnusedAssetsAsync>d__23>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,AssetBundleManager.<UnloadUnusedAssetsAsync>d__23&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,BundleSceneLoader.<LoadSceneAsync>d__7>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,BundleSceneLoader.<LoadSceneAsync>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,BundleSceneLoader.<UnloadSceneAsync>d__8>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,BundleSceneLoader.<UnloadSceneAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,NetworkDownloader.<DownloadAttemptToFileAsync>d__5>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,NetworkDownloader.<DownloadAttemptToFileAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,AssetBundleManager.<AcquireBundleAsync>d__30>(System.Runtime.CompilerServices.TaskAwaiter&,AssetBundleManager.<AcquireBundleAsync>d__30&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,NetworkDownloader.<DownloadToFileAsync>d__4>(System.Runtime.CompilerServices.TaskAwaiter&,NetworkDownloader.<DownloadToFileAsync>d__4&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,UnityHttpClient.<SendAsync>d__0>(System.Runtime.CompilerServices.TaskAwaiter&,UnityHttpClient.<SendAsync>d__0&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AssetBundleManager.<AcquireBundleAsync>d__30>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AssetBundleManager.<AcquireBundleAsync>d__30&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AssetBundleManager.<InternalAcquireBundleAsync>d__31>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AssetBundleManager.<InternalAcquireBundleAsync>d__31&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,AssetBundleManager.<LoadAssetAsync>d__17<object>>(System.Runtime.CompilerServices.TaskAwaiter<object>&,AssetBundleManager.<LoadAssetAsync>d__17<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,NetworkDownloader.<DownloadToMemoryAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,NetworkDownloader.<DownloadToMemoryAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UnityHttpClient.<GetBytesAsync>d__1>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UnityHttpClient.<GetBytesAsync>d__1&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UnityHttpClient.<GetStringAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UnityHttpClient.<GetStringAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UnityHttpClient.<PostAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UnityHttpClient.<PostAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UnityHttpClient.<PostFormAsync>d__4>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UnityHttpClient.<PostFormAsync>d__4&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,UnityHttpClient.<PostJsonAsync>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,UnityHttpClient.<PostJsonAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,AssetBundleManager.<LoadAssetAsync>d__17<object>>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,AssetBundleManager.<LoadAssetAsync>d__17<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,AssetBundleManager.<LoadBundleFileAsync>d__33>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,AssetBundleManager.<LoadBundleFileAsync>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,AssetBundleManager.<LoadManifestBundleFileAsync>d__34>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,AssetBundleManager.<LoadManifestBundleFileAsync>d__34&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter,UnityHttpClient.<SendAsync>d__0>(System.Runtime.CompilerServices.YieldAwaitable.YieldAwaiter&,UnityHttpClient.<SendAsync>d__0&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<AssetBundleManager.<EnsureManifestLoadedAsync>d__28>(AssetBundleManager.<EnsureManifestLoadedAsync>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<AssetBundleManager.<InitializeAsync>d__15>(AssetBundleManager.<InitializeAsync>d__15&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<AssetBundleManager.<LoadManifestAsync>d__29>(AssetBundleManager.<LoadManifestAsync>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<AssetBundleManager.<PreloadBundleAsync>d__19>(AssetBundleManager.<PreloadBundleAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<AssetBundleManager.<UnloadUnusedAssetsAsync>d__23>(AssetBundleManager.<UnloadUnusedAssetsAsync>d__23&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<BundleSceneLoader.<LoadSceneAsync>d__7>(BundleSceneLoader.<LoadSceneAsync>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<BundleSceneLoader.<UnloadSceneAsync>d__8>(BundleSceneLoader.<UnloadSceneAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<GameAssetService.<ReleaseGroupAndCollectAsync>d__33>(GameAssetService.<ReleaseGroupAndCollectAsync>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<NetworkDownloader.<DownloadAttemptToFileAsync>d__5>(NetworkDownloader.<DownloadAttemptToFileAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<NetworkDownloader.<DownloadToStreamAsync>d__3>(NetworkDownloader.<DownloadToStreamAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PreloadService.<ClearAllAsync>d__28>(PreloadService.<ClearAllAsync>d__28&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PreloadService.<EnsureBundleReadyAsync>d__32>(PreloadService.<EnsureBundleReadyAsync>d__32&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PreloadService.<EnsurePackageReadyAsync>d__33>(PreloadService.<EnsurePackageReadyAsync>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PreloadService.<PreloadAssetAsync>d__20<object>>(PreloadService.<PreloadAssetAsync>d__20<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PreloadService.<PreloadAssetsAsync>d__21<object>>(PreloadService.<PreloadAssetsAsync>d__21<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PreloadService.<PreloadBundleAsync>d__18>(PreloadService.<PreloadBundleAsync>d__18&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PreloadService.<PreloadBundlesAsync>d__19>(PreloadService.<PreloadBundlesAsync>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<PreloadService.<PreloadGroupAsync>d__22>(PreloadService.<PreloadGroupAsync>d__22&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<AssetBundleManager.<AcquireBundleAsync>d__30>(AssetBundleManager.<AcquireBundleAsync>d__30&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<AssetBundleManager.<InternalAcquireBundleAsync>d__31>(AssetBundleManager.<InternalAcquireBundleAsync>d__31&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<AssetBundleManager.<LoadAssetAsync>d__17<object>>(AssetBundleManager.<LoadAssetAsync>d__17<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<AssetBundleManager.<LoadBundleFileAsync>d__33>(AssetBundleManager.<LoadBundleFileAsync>d__33&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<AssetBundleManager.<LoadManifestBundleFileAsync>d__34>(AssetBundleManager.<LoadManifestBundleFileAsync>d__34&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<NetworkDownloader.<DownloadToFileAsync>d__4>(NetworkDownloader.<DownloadToFileAsync>d__4&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<NetworkDownloader.<DownloadToMemoryAsync>d__2>(NetworkDownloader.<DownloadToMemoryAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<UnityHttpClient.<GetBytesAsync>d__1>(UnityHttpClient.<GetBytesAsync>d__1&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<UnityHttpClient.<GetStringAsync>d__2>(UnityHttpClient.<GetStringAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<UnityHttpClient.<PostAsync>d__5>(UnityHttpClient.<PostAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<UnityHttpClient.<PostFormAsync>d__4>(UnityHttpClient.<PostFormAsync>d__4&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<UnityHttpClient.<PostJsonAsync>d__3>(UnityHttpClient.<PostJsonAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<UnityHttpClient.<SendAsync>d__0>(UnityHttpClient.<SendAsync>d__0&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,GameAssetService.<Awake>d__17>(System.Runtime.CompilerServices.TaskAwaiter&,GameAssetService.<Awake>d__17&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<GameAssetService.<Awake>d__17>(GameAssetService.<Awake>d__17&)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// UnityEngine.AssetBundleRequest UnityEngine.AssetBundle.LoadAssetAsync<object>(string)
	}
}