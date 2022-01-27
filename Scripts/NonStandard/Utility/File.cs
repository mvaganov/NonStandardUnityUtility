using System.Collections.Generic;
using System.IO;

namespace NonStandard.Utility {
	/// <summary>
	/// can only be used in the Unity Editor
	/// </summary>
	public static class File {
#if UNITY_EDITOR
		public static List<string> FindFile(string absoluteStartingPath, string filename, int allowedRecursion = 0, string[] ignoreFolders = null) {
			string[] list = System.IO.Directory.GetFiles(absoluteStartingPath);
			List<string> result = null;
			string n = System.IO.Path.DirectorySeparatorChar + filename;
			for (int i = 0; i < list.Length; ++i) {
				if (list[i].EndsWith(n)) {
					//UnityEngine.Debug.Log("f " + list[i]);
					if (result == null) { result = new List<string>(); }
					result.Add(list[i]);
				}
			}
			if (allowedRecursion != 0) {
				bool ShouldBeIgnored(string path) {
					if (ignoreFolders == null) return false;
					int index = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
					string folder = path.Substring(index + 1);
					for (int i = 0; i < ignoreFolders.Length; ++i) {
						if (folder.StartsWith(ignoreFolders[i])) return true;
					}
					return false;
				}
				list = System.IO.Directory.GetDirectories(absoluteStartingPath);
				for (int i = 0; i < list.Length; ++i) {
					if (ShouldBeIgnored(list[i])) { continue; }
					//UnityEngine.Debug.Log("d "+list[i]);
					List<string> r = FindFile(list[i], filename, allowedRecursion - 1);
					if (r != null) {
						if (result == null) { result = new List<string>(); }
						result.AddRange(r);
					}
				}
			}
			return result;
		}
		public static void CopyFilesRecursively(string sourcePath, string targetPath) {
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) {
				Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
			}

			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) {
				System.IO.File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
			}
		}
		public static string GetAbsolutePath(string subFolder = null) {
			string startPath = System.IO.Path.GetFullPath(".");
			if (subFolder != null) {
				startPath += System.IO.Path.DirectorySeparatorChar + subFolder;
			}
			return startPath;
		}
		/// <param name="filename">do not include the path, do include the ".cs"</param>
		/// <param name="fileData"></param>
		public static void RewriteAssetCSharpFile(string filename, string fileData, string[] priorityPaths = null) {
			string startPath = System.IO.Path.GetFullPath(".");
			char dir = System.IO.Path.DirectorySeparatorChar;
			string fileBranch = startPath + dir + "Assets";
			//Debug.Log(assetPath);
			List<string> found = FindFile(fileBranch, filename, -1, null);
			if (found == null) {
				fileBranch = startPath + dir + "Packages";
				found = FindFile(fileBranch, filename, -1, null);
			}
			if (found == null) {
				fileBranch = startPath + dir + "Library" + dir + "PackageCache";
				found = FindFile(fileBranch, filename, -1, null);
				if (found != null) {
					UnityEngine.Debug.LogWarning("modifying "+filename+" in Library"+dir+"PackageCache");
				}
			}
			if (found != null) {
				string path = null;
				if(priorityPaths != null) {
					for (int p = 0; p < priorityPaths.Length; p++) {
						for (int i = 0; i < found.Count; i++) {
							if (found[i].Contains(priorityPaths[p])) {
								path = found[i];
								break;
							}
						}
						if (path != null) { break; }
					}
				}
				if (path == null) {
					//Debug.Log(string.Join("\n", found));
					path = found[0];
				}
				System.IO.File.WriteAllText(path, fileData);
				string relativePath = path.Substring(startPath.Length + 1);
				//Debug.Log(relativePath);
				Event.Wait(0, () => {
					UnityEditor.AssetDatabase.ImportAsset(relativePath);
					UnityEditor.EditorGUIUtility.PingObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath(relativePath));
					//UnityEditor.Selection.activeObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath(relativePath);
				});
			} else {
				UnityEngine.Debug.LogError("Could not find C# file "+filename);
			}
		}
#endif
	}
}
