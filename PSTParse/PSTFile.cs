﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using PSTParse.MessageLayer;
using PSTParse.NodeDatabaseLayer;

namespace PSTParse
{
    public class PSTFile : IDisposable
    {
        public string Path { get; set; }
        public MemoryMappedFile PSTMMF { get; set; }
        public PSTHeader Header { get; set; }
        public MailStore MailStore { get; set; }
        public MailFolder TopOfPST { get; set; }
        public NamedToPropertyLookup NamedPropertyLookup { get; set; }
        public double SizeMB => (double)Header.Root.FileSizeBytes / 1000 / 1000;

        public PSTFile(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            PSTMMF = MemoryMappedFile.CreateFromFile(path, FileMode.Open);

            Header = new PSTHeader(this);
            if (Header.IsANSI ?? false || !(Header.IsUNICODE ?? false)) throw new InvalidDataException("PST is ANSI format, currently only support UNICODE");

            /*var messageStoreData = BlockBO.GetNodeData(SpecialNIDs.NID_MESSAGE_STORE);
            var temp = BlockBO.GetNodeData(SpecialNIDs.NID_ROOT_FOLDER);*/
            MailStore = new MailStore(this);

            TopOfPST = new MailFolder(MailStore.RootFolder.NID, new List<string>(), this);
            NamedPropertyLookup = new NamedToPropertyLookup(this);

            //var temp = new TableContext(rootEntryID.NID);
            //PasswordReset.ResetPassword();
        }

        public void CloseMMF()
        {
            PSTMMF.Dispose();
        }

        public void OpenMMF()
        {
            PSTMMF = MemoryMappedFile.CreateFromFile(Path, FileMode.Open);
        }

        public Tuple<ulong, ulong> GetNodeBIDs(ulong NID)
        {
            return Header.NodeBT.Root.GetNIDBID(NID);
        }

        public void Dispose()
        {
            CloseMMF();
        }

        public BBTENTRY GetBlockBBTEntry(ulong item1)
        {
            return Header.BlockBT.Root.GetBIDBBTEntry(item1);
        }
    }
}
