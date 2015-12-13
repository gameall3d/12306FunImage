using UnityEngine;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
	const int MAX_FILE_LENGTH = 2048;
	const int MAX_IMAGE_NUM = 8;

	// gameobject renference
	public RawImage[] showImages;
	public Text InputText;
	public Text ShowText;
	public Text UserNameInput;
	public Text UserNameText;

	public GameObject EditorShowGroup;
	public GameObject PrivewShowGroup;
	public GameObject InfoShowGroup;

	// for game logic
	private Texture defaultTexture1;
	private Texture defaultTexture2;
	private Texture[] defaultTexture = new Texture[MAX_IMAGE_NUM];
	private bool bHasLoadImage = false;

	private Texture2D cutImage;

	string[] GetFileNamesByFileList(List<string> selectedFilesList)
	{
		if (selectedFilesList.Count == 1) 
		{
			// Only one file selected with full path
			return selectedFilesList.ToArray();
		} 
		else 
		{
			// Multiple files selected, add directory
			string[] selectedFiles = new string[selectedFilesList.Count - 1];
			
			for (int i = 0; i < selectedFiles.Length; i++) 
			{
				selectedFiles[i] = selectedFilesList[0] + "\\" + selectedFilesList[i + 1];
			}

			return selectedFiles;
		}
	}

	public void OpenFileDialog()
	{
		OpenFileName ofn = new OpenFileName();
		
		ofn.structSize = Marshal.SizeOf(ofn);
		
		ofn.filter = "All Files\0*.*\0\0";
		
		//ofn.file = new string(new char[256]);
		string fileNames = new String(new char[MAX_FILE_LENGTH]);
		ofn.file = Marshal.StringToBSTR(fileNames);
		
		ofn.maxFile = fileNames.Length;
		
		ofn.fileTitle = new string(new char[64]);
		
		ofn.maxFileTitle = ofn.fileTitle.Length;
		
		ofn.initialDir = UnityEngine.Application.dataPath;//默认路径
		
		ofn.title = "Open Project";
		
		//注意 一下项目不一定要全选 但是0x00000008项不要缺少
		ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
		
		if (DllTest.GetOpenFileName(ofn))
		{
			List<string> selectedFilesList = new List<string>();
			
			long pointer = (long)ofn.file;
			string file = Marshal.PtrToStringAuto(ofn.file);
			
			// Retrieve file names
			while (file.Length > 0) 
			{
				selectedFilesList.Add(file);
				
				pointer += file.Length * 2 + 2;
				ofn.file = (IntPtr)pointer;
				file = Marshal.PtrToStringAuto(ofn.file);
			}
			
			string[] slectedfileNames = GetFileNamesByFileList(selectedFilesList);
			
			for (int i = 0; i < slectedfileNames.Length; i++)
			{
				StartCoroutine(WaitLoad(slectedfileNames[i], i));
			}
			//StartCoroutine(WaitLoad(ofn.file));

			if (slectedfileNames.Length == 6)
			{
				showImages[6].texture = defaultTexture1; 
				showImages[7].texture = defaultTexture2; 
			}
			else if (slectedfileNames.Length == 8)
			{
				showImages[7].texture = defaultTexture2; 
			}
		}
	}

	public void SaveFileDialog()
	{
		OpenFileName ofn = new OpenFileName();
		
		ofn.structSize = Marshal.SizeOf(ofn);
		
		ofn.filter = "All Files\0*.*\0\0";
		
		//ofn.file = new string(new char[256]);
		string fileNames = new String(new char[MAX_FILE_LENGTH]);
		ofn.file = Marshal.StringToBSTR(fileNames);
		
		ofn.maxFile = fileNames.Length;
		
		ofn.fileTitle = new string(new char[64]);
		
		ofn.maxFileTitle = ofn.fileTitle.Length;
		
		ofn.initialDir = UnityEngine.Application.dataPath;//默认路径
		
		ofn.title = "Save Project";
		
		ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
		
		if (DllTest.GetSaveFileName(ofn))
		{
			string file = Marshal.PtrToStringAuto(ofn.file);
			ChangeToPreviewMode();
			StartCoroutine(CutImage(file));
			//ChangeToEidtorMode();
		}
	}

    IEnumerator WaitLoad(string fileName, int idx)
    {
		WWW wwwTexture=new WWW("file://"+fileName);
		
		Debug.Log(wwwTexture.url);
		
		yield return wwwTexture;

		if (idx < showImages.Length)
		{
			showImages[idx].texture = wwwTexture.texture as Texture;
		}
		//showImage.texture = wwwTexture.texture as Texture;
		bHasLoadImage = true;
    }

    IEnumerator WaitSave(string fileName)
    {

        FileInfo fi = new FileInfo(UnityEngine.Application.dataPath + "/Resources/a.txt");
        fi.CopyTo(fileName, true);
        yield return fi;

    }

	// cut image
	IEnumerator CutImage(string path)
	{
		cutImage = new Texture2D (Screen.width, Screen.height, TextureFormat.ARGB32, true);

		Rect rect = new Rect (0, 0, Screen.width, Screen.height);

		yield return new WaitForEndOfFrame ();

		cutImage.ReadPixels (rect, 0, 0, true);
		cutImage.Apply ();
		yield return cutImage;

		byte[] byt = cutImage.EncodeToPNG();  
		//保存截图  
		File.WriteAllBytes(path, byt); 

		ChangeToEidtorMode ();
	}

	private void ChangeToPreviewMode()
	{
		EditorShowGroup.SetActive (false);
		PrivewShowGroup.SetActive (true);
	}

	private void ChangeToEidtorMode()
	{
		EditorShowGroup.SetActive (true);
		PrivewShowGroup.SetActive (false);
	}

	#region events
	// button event
	public void OnClickPreviewButton()
	{
		if (!bHasLoadImage)
		{
			for (int i = 0; i < MAX_IMAGE_NUM; i++)
			{
				showImages[i].texture = defaultTexture[i];
			}

			ShowText.text = "最美语言";
		}

		ChangeToPreviewMode ();
	}

	public void OnClickReturnEditorButton()
	{
		ChangeToEidtorMode ();
	}

	public void OnUserNameInputValueChanged(string name)
	{
		UserNameText.text = name;
	}

	public void OnInputValueChanged(string name)
	{
		Debug.Log(name);
		ShowText.text = name;
	}

	public void OnClickInfoButton()
	{
		EditorShowGroup.SetActive (false);
		InfoShowGroup.SetActive (true);
	}

	public void OnClickBackFromInfo()
	{
		InfoShowGroup.SetActive (false);
		EditorShowGroup.SetActive (true);
	}

	public void OnClickWeiboName()
	{
		Application.OpenURL ("http://weibo.com/1641345092/");
	}
	#endregion events

	void Start()
	{
		Screen.SetResolution(800, 480, false);

		defaultTexture1 = Resources.Load ("haha1") as Texture;
		defaultTexture2 = Resources.Load ("haha2") as Texture;

		defaultTexture [0] = Resources.Load ("programlanguage/c#") as Texture;
		defaultTexture [1] = Resources.Load ("programlanguage/c") as Texture;
		defaultTexture [2] = Resources.Load ("programlanguage/c++") as Texture;
		defaultTexture [3] = Resources.Load ("programlanguage/java") as Texture;
		defaultTexture [4] = Resources.Load ("programlanguage/javascript") as Texture;
		defaultTexture [5] = Resources.Load ("programlanguage/lua") as Texture;
		defaultTexture [6] = Resources.Load ("programlanguage/php") as Texture;
		defaultTexture [7] = Resources.Load ("programlanguage/python") as Texture;
	}


}