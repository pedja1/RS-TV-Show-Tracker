﻿//
//  Based on http://mftscanner.codeplex.com/SourceControl/changeset/view/2132#67339
//  Modifications:
//   - Removed some of the functions which weren't used in this software
//   - Commented, refactored and removed some small parts of the code
//   - Modified GetPathFromFileReference() to build up the full path
//     by navigating through the reference IDs, instead of using a Win32
//     API call, like the original does. This is much fucking faster.
//   - Merged GetNtfsVolumeFolders() and GetFilesMatchingFilter() so
//     you won't have to iterate through the MFT twice to get the
//     entries for the folders and files separately.
//   - Added optional Regex-based filter.
//  
//  Big thanks to StCroixSkipper!
//

namespace RoliSoft.TVShowTracker.Dependencies.USNJournal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Accesses and parses the NTFS Master File Table entries.
    /// </summary>
    public class NtfsUsnJournal : IDisposable
    {
        public enum UsnJournalReturnCode
        {
            INVALID_HANDLE_VALUE = -1,
            USN_JOURNAL_SUCCESS = 0,
            ERROR_INVALID_FUNCTION = 1,
            ERROR_FILE_NOT_FOUND = 2,
            ERROR_PATH_NOT_FOUND = 3,
            ERROR_TOO_MANY_OPEN_FILES = 4,
            ERROR_ACCESS_DENIED = 5,
            ERROR_INVALID_HANDLE = 6,
            ERROR_INVALID_DATA = 13,
            ERROR_HANDLE_EOF = 38,
            ERROR_NOT_SUPPORTED = 50,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_JOURNAL_DELETE_IN_PROGRESS = 1178,
            USN_JOURNAL_NOT_ACTIVE = 1179,
            ERROR_JOURNAL_ENTRY_DELETED = 1181,
            ERROR_INVALID_USER_BUFFER = 1784,
            USN_JOURNAL_INVALID = 17001,
            VOLUME_NOT_NTFS = 17003,
            INVALID_FILE_REFERENCE_NUMBER = 17004,
            USN_JOURNAL_ERROR = 17005
        }

        public enum UsnReasonCode
        {
            USN_REASON_DATA_OVERWRITE = 0x00000001,
            USN_REASON_DATA_EXTEND = 0x00000002,
            USN_REASON_DATA_TRUNCATION = 0x00000004,
            USN_REASON_NAMED_DATA_OVERWRITE = 0x00000010,
            USN_REASON_NAMED_DATA_EXTEND = 0x00000020,
            USN_REASON_NAMED_DATA_TRUNCATION = 0x00000040,
            USN_REASON_FILE_CREATE = 0x00000100,
            USN_REASON_FILE_DELETE = 0x00000200,
            USN_REASON_EA_CHANGE = 0x00000400,
            USN_REASON_SECURITY_CHANGE = 0x00000800,
            USN_REASON_RENAME_OLD_NAME = 0x00001000,
            USN_REASON_RENAME_NEW_NAME = 0x00002000,
            USN_REASON_INDEXABLE_CHANGE = 0x00004000,
            USN_REASON_BASIC_INFO_CHANGE = 0x00008000,
            USN_REASON_HARD_LINK_CHANGE = 0x00010000,
            USN_REASON_COMPRESSION_CHANGE = 0x00020000,
            USN_REASON_ENCRYPTION_CHANGE = 0x00040000,
            USN_REASON_OBJECT_ID_CHANGE = 0x00080000,
            USN_REASON_REPARSE_POINT_CHANGE = 0x00100000,
            USN_REASON_STREAM_CHANGE = 0x00200000,
            USN_REASON_CLOSE = -1
        }

        private readonly DriveInfo _driveInfo;
        private readonly uint _volumeSerialNumber;
        private IntPtr _usnJournalRootHandle;

        /// <summary>
        /// Constructor for NtfsUsnJournal class.  If no exception is thrown, _usnJournalRootHandle and
        /// _volumeSerialNumber can be assumed to be good. If an exception is thrown, the NtfsUsnJournal
        /// object is not usable.
        /// </summary>
        /// <param name="driveInfo">DriveInfo object that provides access to information about a volume</param>
        /// <remarks> 
        /// An exception thrown if the volume is not an 'NTFS' volume or
        /// if GetRootHandle() or GetVolumeSerialNumber() functions fail. 
        /// Each public method checks to see if the volume is NTFS and if the _usnJournalRootHandle is
        /// valid.  If these two conditions aren't met, then the public function will return a UsnJournalReturnCode
        /// error.
        /// </remarks>
        public NtfsUsnJournal(DriveInfo driveInfo)
        {
            _driveInfo = driveInfo;

            if (_driveInfo.DriveFormat != "NTFS")
            {
                throw new Exception(string.Format("{0} is not an 'NTFS' volume.", _driveInfo.Name));
            }

            var rootHandle = IntPtr.Zero;
            var usnRtnCode = GetRootHandle(out rootHandle);

            if (usnRtnCode != UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
            {
                throw new Win32Exception((int) usnRtnCode);
            }

            _usnJournalRootHandle = rootHandle;

            usnRtnCode = GetVolumeSerialNumber(_driveInfo, out _volumeSerialNumber);
            if (usnRtnCode != UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
            {
                throw new Win32Exception((int)usnRtnCode);
            }
        }

        /// <summary>
        /// GetFileAndDirEntries() reads the Master File Table to find all of the files and
        /// folders on a volume and returns them individually.
        /// </summary>
        /// <returns>
        /// USN_JOURNAL_SUCCESS                 GetNtfsVolumeFolders() function succeeded. 
        /// VOLUME_NOT_NTFS                     volume is not an NTFS volume.
        /// INVALID_HANDLE_VALUE                NtfsUsnJournal object failed initialization.
        /// USN_JOURNAL_NOT_ACTIVE              usn journal is not active on volume.
        /// ERROR_ACCESS_DENIED                 accessing the usn journal requires admin rights, see remarks.
        /// ERROR_INVALID_FUNCTION              error generated by DeviceIoControl() call.
        /// ERROR_FILE_NOT_FOUND                error generated by DeviceIoControl() call.
        /// ERROR_PATH_NOT_FOUND                error generated by DeviceIoControl() call.
        /// ERROR_TOO_MANY_OPEN_FILES           error generated by DeviceIoControl() call.
        /// ERROR_INVALID_HANDLE                error generated by DeviceIoControl() call.
        /// ERROR_INVALID_DATA                  error generated by DeviceIoControl() call.
        /// ERROR_NOT_SUPPORTED                 error generated by DeviceIoControl() call.
        /// ERROR_INVALID_PARAMETER             error generated by DeviceIoControl() call.
        /// ERROR_JOURNAL_DELETE_IN_PROGRESS    usn journal delete is in progress.
        /// ERROR_INVALID_USER_BUFFER           error generated by DeviceIoControl() call.
        /// USN_JOURNAL_ERROR                   unspecified usn journal error.
        /// </returns>
        /// <remarks>
        /// If function returns ERROR_ACCESS_DENIED you need to run application as an Administrator.
        /// </remarks>
        public UsnJournalReturnCode GetFileAndDirEntries(out Dictionary<ulong, Win32Api.UsnEntry> dirs, out List<Win32Api.UsnEntry> files, Regex filter = null)
        {
            dirs  = new Dictionary<ulong, Win32Api.UsnEntry>();
            files = new List<Win32Api.UsnEntry>();

            if (_usnJournalRootHandle.ToInt32() == Win32Api.INVALID_HANDLE_VALUE)
            {
                return UsnJournalReturnCode.INVALID_HANDLE_VALUE;
            }

            var usnState   = new Win32Api.USN_JOURNAL_DATA();
            var usnRtnCode = QueryUsnJournal(ref usnState);

            if (usnRtnCode != UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
            {
                return usnRtnCode;
            }

            //
            // set up MFT_ENUM_DATA structure
            //
            Win32Api.MFT_ENUM_DATA med;
            med.StartFileReferenceNumber = 0;
            med.LowUsn = 0;
            med.HighUsn = usnState.NextUsn;
            Int32 sizeMftEnumData = Marshal.SizeOf(med);
            IntPtr medBuffer = Marshal.AllocHGlobal(sizeMftEnumData);
            Win32Api.ZeroMemory(medBuffer, sizeMftEnumData);
            Marshal.StructureToPtr(med, medBuffer, true);

            //
            // set up the data buffer which receives the USN_RECORD data
            //
            int pDataSize = sizeof (UInt64) + 10000;
            IntPtr pData = Marshal.AllocHGlobal(pDataSize);
            Win32Api.ZeroMemory(pData, pDataSize);
            uint outBytesReturned = 0;
            Win32Api.UsnEntry usnEntry = null;

            //
            // Gather up volume's directories
            //
            while (false != Win32Api.DeviceIoControl(
                _usnJournalRootHandle,
                Win32Api.FSCTL_ENUM_USN_DATA,
                medBuffer,
                sizeMftEnumData,
                pData,
                pDataSize,
                out outBytesReturned,
                IntPtr.Zero))
            {
                IntPtr pUsnRecord = new IntPtr(pData.ToInt32() + sizeof (Int64));
                while (outBytesReturned > 60)
                {
                    usnEntry = new Win32Api.UsnEntry(pUsnRecord);

                    if (usnEntry.IsFile && (filter == null || filter.IsMatch(usnEntry.Name)))
                    {
                        files.Add(usnEntry);
                    }
                    if (usnEntry.IsFolder)
                    {
                        dirs.Add(usnEntry.FileReferenceNumber, usnEntry);
                    }

                    pUsnRecord = new IntPtr(pUsnRecord.ToInt32() + usnEntry.RecordLength);
                    outBytesReturned -= usnEntry.RecordLength;
                }
                Marshal.WriteInt64(medBuffer, Marshal.ReadInt64(pData, 0));
            }

            Marshal.FreeHGlobal(pData);
            usnRtnCode = ConvertWin32ErrorToUsnError((Win32Api.GetLastErrorEnum) Marshal.GetLastWin32Error());
            if (usnRtnCode == UsnJournalReturnCode.ERROR_HANDLE_EOF)
            {
                usnRtnCode = UsnJournalReturnCode.USN_JOURNAL_SUCCESS;
            }

            return usnRtnCode;
        }

        /// <summary>
        /// Calls GetFileAndDirEntries() internally, and matches all of the file entries to their
        /// parent directory entries recursively, so it can generate a full path way much faster
        /// than the original function did in this library which placed Win32 API calls.
        /// </summary>
        /// <returns>
        /// List of all the files on the volume including their full path.
        /// </returns>
        public IEnumerable<string> GetParsedPaths(Regex filter = null, IEnumerable<string> pathFilters = null)
        {
            Dictionary<ulong, Win32Api.UsnEntry> dirs;
            List<Win32Api.UsnEntry> files;

            var usnRtnCode = GetFileAndDirEntries(out dirs, out files, filter);

            if (usnRtnCode != UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
            {
                throw new Exception(usnRtnCode.ToString());
            }

            foreach (var file in files)
            {
                var names   = new Stack<string>();
                var current = file;

                while (true)
                {
                    names.Push(current.Name);

                    if (!dirs.TryGetValue(current.ParentFileReferenceNumber, out current))
                    {
                        break;
                    }
                }

                var name = _driveInfo.Name + string.Join(@"\", names);

                if (pathFilters == null || pathFilters.Any(pf => name.StartsWith(pf, StringComparison.InvariantCultureIgnoreCase)))
                {
                    yield return name;
                }
            }
        }

        /// <summary>
        /// Converts a Win32 Error to a UsnJournalReturnCode
        /// </summary>
        /// <param name="Win32LastError">The 'last' Win32 error.</param>
        /// <returns>
        /// INVALID_HANDLE_VALUE                error generated by Win32 Api calls.
        /// USN_JOURNAL_SUCCESS                 usn journal function succeeded. 
        /// ERROR_INVALID_FUNCTION              error generated by Win32 Api calls.
        /// ERROR_FILE_NOT_FOUND                error generated by Win32 Api calls.
        /// ERROR_PATH_NOT_FOUND                error generated by Win32 Api calls.
        /// ERROR_TOO_MANY_OPEN_FILES           error generated by Win32 Api calls.
        /// ERROR_ACCESS_DENIED                 accessing the usn journal requires admin rights.
        /// ERROR_INVALID_HANDLE                error generated by Win32 Api calls.
        /// ERROR_INVALID_DATA                  error generated by Win32 Api calls.
        /// ERROR_HANDLE_EOF                    error generated by Win32 Api calls.
        /// ERROR_NOT_SUPPORTED                 error generated by Win32 Api calls.
        /// ERROR_INVALID_PARAMETER             error generated by Win32 Api calls.
        /// ERROR_JOURNAL_DELETE_IN_PROGRESS    usn journal delete is in progress.
        /// ERROR_JOURNAL_ENTRY_DELETED         usn journal entry lost, no longer available.
        /// ERROR_INVALID_USER_BUFFER           error generated by Win32 Api calls.
        /// USN_JOURNAL_INVALID                 usn journal is invalid, id's don't match or required entries lost.
        /// USN_JOURNAL_NOT_ACTIVE              usn journal is not active on volume.
        /// VOLUME_NOT_NTFS                     volume is not an NTFS volume.
        /// INVALID_FILE_REFERENCE_NUMBER       bad file reference number - see remarks.
        /// USN_JOURNAL_ERROR                   unspecified usn journal error.
        /// </returns>
        private UsnJournalReturnCode ConvertWin32ErrorToUsnError(Win32Api.GetLastErrorEnum Win32LastError)
        {
            UsnJournalReturnCode usnRtnCode;

            switch (Win32LastError)
            {
                case Win32Api.GetLastErrorEnum.ERROR_JOURNAL_NOT_ACTIVE:
                    usnRtnCode = UsnJournalReturnCode.USN_JOURNAL_NOT_ACTIVE;
                    break;
                case Win32Api.GetLastErrorEnum.ERROR_SUCCESS:
                    usnRtnCode = UsnJournalReturnCode.USN_JOURNAL_SUCCESS;
                    break;
                case Win32Api.GetLastErrorEnum.ERROR_HANDLE_EOF:
                    usnRtnCode = UsnJournalReturnCode.ERROR_HANDLE_EOF;
                    break;
                default:
                    usnRtnCode = UsnJournalReturnCode.USN_JOURNAL_ERROR;
                    break;
            }

            return usnRtnCode;
        }

        /// <summary>
        /// Gets a Volume Serial Number for the volume represented by driveInfo.
        /// </summary>
        /// <param name="driveInfo">DriveInfo object representing the volume in question.</param>
        /// <param name="volumeSerialNumber">out parameter to hold the volume serial number.</param>
        /// <returns></returns>
        private UsnJournalReturnCode GetVolumeSerialNumber(DriveInfo driveInfo, out uint volumeSerialNumber)
        {
            volumeSerialNumber = 0;
            UsnJournalReturnCode usnRtnCode = UsnJournalReturnCode.USN_JOURNAL_SUCCESS;
            string pathRoot = string.Concat("\\\\.\\", driveInfo.Name);

            IntPtr hRoot = Win32Api.CreateFile(pathRoot,
                0,
                Win32Api.FILE_SHARE_READ | Win32Api.FILE_SHARE_WRITE,
                IntPtr.Zero,
                Win32Api.OPEN_EXISTING,
                Win32Api.FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero);

            if (hRoot.ToInt32() != Win32Api.INVALID_HANDLE_VALUE)
            {
                Win32Api.BY_HANDLE_FILE_INFORMATION fi = new Win32Api.BY_HANDLE_FILE_INFORMATION();
                bool bRtn = Win32Api.GetFileInformationByHandle(hRoot, out fi);

                if (bRtn)
                {
                    UInt64 fileIndexHigh = (UInt64)fi.FileIndexHigh;
                    UInt64 indexRoot = (fileIndexHigh << 32) | fi.FileIndexLow;
                    volumeSerialNumber = fi.VolumeSerialNumber;
                }
                else
                {
                    usnRtnCode = (UsnJournalReturnCode)Marshal.GetLastWin32Error();
                }

                Win32Api.CloseHandle(hRoot);
            }
            else
            {
                usnRtnCode = (UsnJournalReturnCode)Marshal.GetLastWin32Error();
            }

            return usnRtnCode;
        }

        /// <summary>
        /// Gets the root handle.
        /// </summary>
        /// <param name="rootHandle">The root handle.</param>
        /// <returns></returns>
        private UsnJournalReturnCode GetRootHandle(out IntPtr rootHandle)
        {
            UsnJournalReturnCode usnRtnCode = UsnJournalReturnCode.USN_JOURNAL_SUCCESS;
            rootHandle = IntPtr.Zero;
            string vol = string.Concat("\\\\.\\", _driveInfo.Name.TrimEnd('\\'));

            rootHandle = Win32Api.CreateFile(vol,
                 Win32Api.GENERIC_READ | Win32Api.GENERIC_WRITE,
                 Win32Api.FILE_SHARE_READ | Win32Api.FILE_SHARE_WRITE,
                 IntPtr.Zero,
                 Win32Api.OPEN_EXISTING,
                 0,
                 IntPtr.Zero);

            if (rootHandle.ToInt32() == Win32Api.INVALID_HANDLE_VALUE)
            {
                usnRtnCode = (UsnJournalReturnCode)Marshal.GetLastWin32Error();
            }

            return usnRtnCode;
        }

        /// <summary>
        /// This function queries the usn journal on the volume. 
        /// </summary>
        /// <param name="usnJournalState">the USN_JOURNAL_DATA object that is associated with this volume</param>
        /// <returns></returns>
        private UsnJournalReturnCode QueryUsnJournal(ref Win32Api.USN_JOURNAL_DATA usnJournalState)
        {
            UsnJournalReturnCode usnReturnCode = UsnJournalReturnCode.USN_JOURNAL_SUCCESS;
            int sizeUsnJournalState = Marshal.SizeOf(usnJournalState);
            UInt32 cb;

            var fOk = Win32Api.DeviceIoControl(
                _usnJournalRootHandle,
                Win32Api.FSCTL_QUERY_USN_JOURNAL,
                IntPtr.Zero,
                0,
                out usnJournalState,
                sizeUsnJournalState,
                out cb,
                IntPtr.Zero);

            if (!fOk)
            {
                int lastWin32Error = Marshal.GetLastWin32Error();
                usnReturnCode = ConvertWin32ErrorToUsnError((Win32Api.GetLastErrorEnum)Marshal.GetLastWin32Error());
            }

            return usnReturnCode;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Win32Api.CloseHandle(_usnJournalRootHandle);
        }
    }
}

