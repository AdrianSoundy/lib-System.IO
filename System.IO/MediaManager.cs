//
// Copyright (c) 2020 The nanoFramework project contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//
using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;

using nanoFramework.IO;
using nanoFramework.Runtime.Events;

namespace System.IO
{
    internal enum StorageEventType : byte
    {
        Invalid = 0,
        Insert = 1,
        Eject = 2,
    }

    internal class StorageEvent : BaseEvent
    {
        public StorageEventType EventType;
        public uint Handle;
        public DateTime Time;
    }

    internal class StorageEventProcessor : IEventProcessor
    {
        public BaseEvent ProcessEvent(uint data1, uint data2, DateTime time)
        {
            StorageEvent ev = new StorageEvent();
            ev.EventType = (StorageEventType)(data1 & 0xFF);
            ev.Handle = data2;
            ev.Time = time;

            return ev;
        }
    }

    internal class StorageEventListener : IEventListener
    {
        public void InitializeForEventSource()
        {
        }

        public bool OnEvent(BaseEvent ev)
        {
            if (ev is StorageEvent)
            {
                RemovableMedia.PostEvent((StorageEvent)ev);
            }

            return true;
        }
    }

    /// <summary>
    /// Encapsulates information about a file-system volume.
    /// </summary>
    public sealed class VolumeInfo
    {
        /// <summary>
        /// Provides the name of the volume.
        /// </summary>
        public readonly String Name;
        /// <summary>
        /// Provides the label assigned to this volume by a user.
        /// </summary>
        public readonly String VolumeLabel;
        /// <summary>
        /// Provides the ID number associated with this volume.
        /// </summary>
        public readonly uint VolumeID;
        /// <summary>
        /// The file system used on the volume.
        /// </summary>
        public readonly String FileSystem;
        /// <summary>
        /// Flags containing information about the file system used on the volume.
        /// </summary>
        public readonly uint FileSystemFlags;
        /// <summary>
        /// Flags containing information about the volume device.
        /// </summary>
        public readonly uint DeviceFlags;
        /// <summary>
        /// Provides the serial number of this volume.
        /// </summary>
        public readonly uint SerialNumber;
        /// <summary>
        /// Provides the total free space available on this volume, in bytes.
        /// </summary>
        public readonly long TotalFreeSpace;
        /// <summary>
        /// Provides the total size available for content on this volume, in bytes.
        /// </summary>
        public readonly long TotalSize;

        internal uint VolumePtr;

        /// <summary>
        /// Initializes a new instance of the VolumeInfo class.
        /// </summary>
        /// <param name="volumeName">The name of the volume.</param>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern VolumeInfo(String volumeName);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern VolumeInfo(uint volumePtr);

        // This is used internally to create a VolumeInfo for removable volumes that have been ejected
        internal VolumeInfo(VolumeInfo ejectedVolumeInfo)
        {
            Name = ejectedVolumeInfo.Name;
        }

        /// <summary>
        /// Checks the hardware to refresh information about volumes.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void Refresh();

        /// <summary>
        /// Gets the root directory on this volume.
        /// </summary>
        public String RootDirectory
        {
            get { return "\\" + Name; }
        }

        /// <summary>
        /// Determines whether this volume is already formatted or not.
        /// </summary>
        public bool IsFormatted
        {
            get { return FileSystem != null && TotalSize > 0; }
        }

        /// <summary>
        /// Formats this volume using the file system designated in the FileSystem field, 
        /// without forcing removal of the rooted namespace of the volume.
        /// </summary>
        /// <param name="parameter"></param>
        public void Format(uint parameter)
        {
            Format(FileSystem, parameter, VolumeLabel, false);
        }

        /// <summary>
        /// Formats this volume using the file system designated in the FileSystem field and optionally 
        /// forces removal of the rooted namespace of the volume.
        /// </summary>
        /// <param name="parameter">A parameter to guide formatting for the specified file system.</param>
        /// <param name="force">true to force removal of the rooted namespace of this volume; otherwise, false.</param>
        public void Format(uint parameter, bool force)
        {
            Format(FileSystem, parameter, VolumeLabel, force);
        }

        /// <summary>
        /// Formats this volume, without forcing removal of the rooted namespace of the volume.
        /// </summary>
        /// <param name="fileSystem">The name of the file system to use in formatting.</param>
        /// <param name="parameter">A parameter to guide formatting for the specified file system.</param>
        public void Format(String fileSystem, uint parameter)
        {
            Format(fileSystem, parameter, VolumeLabel, false);
        }

        /// <summary>
        /// Formats this volume with the specified volume label, and optionally forces removal of the rooted namespace of the volume.
        /// </summary>
        /// <param name="fileSystem">The name of the file system to use in formatting.</param>
        /// <param name="parameter">A parameter to guide formatting for the specified file system.</param>
        /// <param name="force">true to force removal of the rooted namespace of this volume; otherwise, false.</param>
        public void Format(String fileSystem, uint parameter, bool force)
        {
            Format( fileSystem, parameter, VolumeLabel, force );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileSystem">The name of the file system to use in formatting.</param>
        /// <param name="parameter">A parameter to guide formatting for the specified file system.</param>
        /// <param name="volumeLabel">The label to assign to this volume.</param>
        /// <param name="force">true to force removal of the rooted namespace of this volume; otherwise, false.</param>
        public void Format(String fileSystem, uint parameter, String volumeLabel, bool force )
        {
            String rootedNameSpace = "\\" + Name;

            bool restoreCD = FileSystemManager.CurrentDirectory == rootedNameSpace;

            if (FileSystemManager.IsInDirectory(FileSystemManager.CurrentDirectory, rootedNameSpace))
            {
                FileSystemManager.SetCurrentDirectory(NativeIO.FSRoot);
            }

            if (force)
            {
                FileSystemManager.ForceRemoveNameSpace(Name);
            }

            Object record = FileSystemManager.LockDirectory(rootedNameSpace);

            try
            {
                NativeIO.Format(Name, fileSystem, volumeLabel, parameter);
                Refresh();
            }
            finally
            {
                FileSystemManager.UnlockDirectory(record);
            }

            if (restoreCD)
            {
                FileSystemManager.SetCurrentDirectory(rootedNameSpace);
            }
        }

        /// <summary>
        /// Gets a list of known volumes.
        /// </summary>
        /// <returns></returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static VolumeInfo[] GetVolumes();

        /// <summary>
        /// Gets a list of the available file systems.
        /// </summary>
        /// <returns></returns>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern static String[] GetFileSystems();

        /// <summary>
        /// Flushes all information.
        /// </summary>
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern void FlushAll();
    }

    /// <summary>
    /// Support for REmovable media Insert and Eject events
    /// </summary>
    public static class RemovableMedia
    {
        /// <summary>
        /// Media Inserted Event Handler.
        /// </summary>
        public static event InsertEventHandler Insert;
        /// <summary>
        /// Media Ejected event handler.
        /// </summary>
        public static event EjectEventHandler Eject;

        private static ArrayList _volumes;
        private static Queue _events;

        //--//
        
        static RemovableMedia()
        {
            try
            {
                // FIXME EventCategory Storage
                //                 nanoFramework.Runtime.Events.EventSink.AddEventProcessor(EventCategory.Storage, new StorageEventProcessor());
                //                nanoFramework.Runtime.Events.EventSink.AddEventListener(EventCategory.Storage, new StorageEventListener());

                _volumes = new ArrayList();
                _events = new Queue();


                MountRemovableVolumes();
            }
            catch
            {

            }
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static void MountRemovableVolumes();

        internal static void PostEvent(StorageEvent ev)
        {
            // We are using timer to process events instead of a separate message loop
            // thread, to keep it light weight.
            try
            {
                // create a time and push it in a queue to make sure GC does not eat it up before it fires
                // the timer will pop itself in the callback and GC will take it from there
                // be trasaction-safe and create and queue the timer before setting it up to fire
                Timer messagePseudoThread = new Timer(MessageHandler, ev, Timeout.Infinite, Timeout.Infinite);
                _events.Enqueue(messagePseudoThread);
                // now that all operation that can fail have been done, enable the timer to fire
                messagePseudoThread.Change(10, Timeout.Infinite);
            }
            catch // eat up all exceptions, nothing we can do
            {
            }
        }

        private static void MessageHandler(object args)
        {
            try
            {
                StorageEvent ev = args as StorageEvent;

                if (ev == null)
                    return;

                lock(_volumes)
                {
                    if (ev.EventType == StorageEventType.Insert)
                    {
                        VolumeInfo volume = new VolumeInfo(ev.Handle);

                        _volumes.Add(volume);

                        if (Insert != null)
                        {
                            MediaEventArgs mediaEventArgs = new MediaEventArgs(volume, ev.Time);

                            Insert(null, mediaEventArgs);
                        }
                    }
                    else if (ev.EventType == StorageEventType.Eject)
                    {
                        VolumeInfo volumeInfo = RemoveVolume(ev.Handle);

                        if(volumeInfo != null) 
                        {
                            FileSystemManager.ForceRemoveNameSpace(volumeInfo.Name);

                            if (Eject != null)
                            {
                                MediaEventArgs mediaEventArgs = new MediaEventArgs(new VolumeInfo(volumeInfo), ev.Time);

                                Eject(null, mediaEventArgs);
                            }
                        }
                    }
                }
            }           
            finally
            {
                // get rid of this timer
                _events.Dequeue();    
            }
        }

        private static VolumeInfo RemoveVolume(uint handle)
        {
            VolumeInfo volumeInfo;
            int count = _volumes.Count;

            for (int i = 0; i < count; i++)
            {
                volumeInfo = ((VolumeInfo)_volumes[i]);
                if (volumeInfo.VolumePtr == handle)
                {
                    _volumes.RemoveAt(i);
                    return volumeInfo;
                }
            }

            return null;
        }
    }

    //--//

    /// <summary>
    /// Contains data about a media-related event.
    /// </summary>
    public class MediaEventArgs
    {
        /// <summary>
        /// The time at which the event occurred.
        /// </summary>
        public readonly DateTime Time;
        /// <summary>
        /// The volume that raised the event.
        /// </summary>
        public readonly VolumeInfo Volume;

        /// <summary>
        /// Initializes a new instance of the MediaEventArgs class.
        /// </summary>
        /// <param name="volume">The volume identifier value associated with the event.</param>
        /// <param name="time">The time at which the event occurred.</param>
        public MediaEventArgs(VolumeInfo volume, DateTime time)
        {
            Time = time;
            Volume = volume;
        }
    }

    /// <summary>
    /// Insert event handler delegate
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">MediaEventArgs for event.</param>
    public delegate void InsertEventHandler(object sender, MediaEventArgs e);
    /// <summary>
    /// Eject event handler delegate.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">MediaEventArgs for event.</param>
    public delegate void EjectEventHandler(object sender, MediaEventArgs e);
}


