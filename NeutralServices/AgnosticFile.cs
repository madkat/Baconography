
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if !WINDOWS_PHONE
using Windows.Storage;
#endif
using Windows.System.Threading;

namespace Baconography.NeutralServices
{

	class AgnosticFile : IDisposable
	{
#if WINDOWS_PHONE
		string _file;
#else
		Stream _fileStream;
		StorageFolder _folder;
		StorageFile _file;
		string _fileName;
		string _folderPath;
#endif

		public AgnosticFile(string file)
		{
#if WINDOWS_PHONE
			_file = file;
#else
			_folderPath = file.Substring(0, file.LastIndexOf('\\'));
			_fileName = file.Substring(file.LastIndexOf('\\') + 1);
#endif
		}

		public void Dispose()
		{
#if WINDOWS_PHONE
		
#else
			if (_fileStream != null)
				_fileStream.Dispose();
			_folder = null;
			_file = null;
			_fileName = null;
#endif
		}

#if !WINDOWS_PHONE
		private async Task InitFolder()
		{
			_folder = await StorageFolder.GetFolderFromPathAsync(_folderPath);
		}
#endif

		public async Task<bool> Exists()
		{
#if WINDOWS_PHONE
			return File.Exists(_file);
#else
			try
			{
				await InitFolder();
				if (_file != null)
					return true;

				_file = await _folder.GetFileAsync(_fileName);
				return true;
			}
			catch (FileNotFoundException fnf)
			{
				return false;
			}
#endif
		}

		public async Task<StreamReader> GetStreamReader()
		{
#if WINDOWS_PHONE
			return File.OpenText(_file);
#else
			try
			{
				await InitFolder();
				if (_file == null)
					_file = await _folder.GetFileAsync(_fileName);

				if (_fileStream != null)
					_fileStream.Dispose();

				_fileStream = await _file.OpenStreamForReadAsync();
				return new StreamReader(_fileStream);
			}
			catch (FileNotFoundException fnf)
			{
				return null;
			}
#endif
		}

		public async Task<StreamWriter> GetStreamWriter(bool forAppend = false)
		{
#if WINDOWS_PHONE
			if (forAppend)
				return File.AppendText(_file);
			else
				return new StreamWriter(File.OpenWrite(_file));
#else
			await Create();

			if (_fileStream != null)
				_fileStream.Dispose();

			var randomAccessStream = await _file.OpenAsync(FileAccessMode.ReadWrite);
			if (forAppend)
				_fileStream = randomAccessStream.GetOutputStreamAt(randomAccessStream.Size).AsStreamForWrite();
			else
				_fileStream = randomAccessStream.GetOutputStreamAt(0).AsStreamForWrite();
			return new StreamWriter(_fileStream);
#endif
		}

		public async Task Delete()
		{
#if WINDOWS_PHONE
			if (File.Exists(_file))
				File.Delete(_file);
#else
			if (await Exists())
				await _file.DeleteAsync();
#endif
		}

        public async Task Rename(string newName)
        {
#if WINDOWS_PHONE
            File.Move(_file, newName);
#else
            await _file.RenameAsync(newName);
#endif
        }

		public async Task Create()
		{
#if WINDOWS_PHONE
			File.CreateText(_file).Dispose();
#else
			if (await Exists())
				return;

			try
			{
				_file = await _folder.CreateFileAsync(_fileName);
			}
			catch
			{

			}
#endif
		}
	}
}