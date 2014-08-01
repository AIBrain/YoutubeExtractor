// ****************************************************************************
//
// FLV Extract
// Copyright (C) 2006-2012  J.D. Purcell (moitah@yahoo.com)
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// ****************************************************************************

using System;
using System.IO;

namespace YoutubeExtractor {

    internal class FlvFile : IDisposable {
        private readonly long _fileLength;
        private readonly string _inputPath;
        private readonly string _outputPath;
        private IAudioExtractor _audioExtractor;
        private long _fileOffset;
        private FileStream _fileStream;

        public bool ExtractedAudio { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlvFile"/> class.
        /// </summary>
        /// <param name="inputPath">The path of the input.</param>
        /// <param name="outputPath">The path of the output without extension.</param>
        public FlvFile( string inputPath, string outputPath ) {
            this._inputPath = inputPath;
            this._outputPath = outputPath;
            this._fileStream = new FileStream( this._inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024 );
            this._fileOffset = 0;
            this._fileLength = this._fileStream.Length;
        }

        public void Dispose() {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        private void Dispose( bool disposing ) {
            if ( disposing ) {
                if ( this._fileStream != null ) {
                    this._fileStream.Close();
                    this._fileStream = null;
                }

                this.CloseOutput( true );
            }
        }

        private void CloseOutput( bool disposing ) {
            if ( this._audioExtractor == null ) {
                return;
            }
            if ( disposing && this._audioExtractor.VideoPath != null ) {
                try {
                    File.Delete( this._audioExtractor.VideoPath );
                }
                catch { }
            }

            this._audioExtractor.Dispose();
            this._audioExtractor = null;
        }

        /// <exception cref="AudioExtractionException">The input file is not an FLV file.</exception>
        public void ExtractStreams() {
            this.Seek( 0 );

            if ( this.ReadUInt32() != 0x464C5601 ) {
                // not a FLV file
                throw new AudioExtractionException( "Invalid input file. Impossible to extract audio track." );
            }

            this.ReadUInt8();
            var dataOffset = this.ReadUInt32();

            this.Seek( dataOffset );

            this.ReadUInt32();

            while ( this._fileOffset < this._fileLength ) {
                if ( !this.ReadTag() ) {
                    break;
                }

                if ( this._fileLength - this._fileOffset < 4 ) {
                    break;
                }

                this.ReadUInt32();

                var progress = ( this._fileOffset * 1.0 / this._fileLength ) * 100;

                if ( this.ConversionProgressChanged != null ) {
                    this.ConversionProgressChanged( this, new ProgressEventArgs( progress ) );
                }
            }

            this.CloseOutput( false );
        }

        private void Seek( long offset ) {
            this._fileStream.Seek( offset, SeekOrigin.Begin );
            this._fileOffset = offset;
        }

        private uint ReadUInt32() {
            var x = new byte[ 4 ];

            this._fileStream.Read( x, 0, 4 );
            this._fileOffset += 4;

            return BigEndianBitConverter.ToUInt32( x, 0 );
        }

        private uint ReadUInt8() {
            this._fileOffset += 1;
            return ( uint )this._fileStream.ReadByte();
        }

        private bool ReadTag() {
            if ( this._fileLength - this._fileOffset < 11 )
                return false;

            // Read tag header
            var tagType = this.ReadUInt8();
            var dataSize = this.ReadUInt24();
            var timeStamp = this.ReadUInt24();
            timeStamp |= this.ReadUInt8() << 24;
            this.ReadUInt24();

            // Read tag data
            if ( dataSize == 0 )
                return true;

            if ( this._fileLength - this._fileOffset < dataSize )
                return false;

            var mediaInfo = this.ReadUInt8();
            dataSize -= 1;
            var data = this.ReadBytes( ( int )dataSize );

            if ( tagType == 0x8 ) {
                // If we have no audio writer, create one
                if ( this._audioExtractor == null ) {
                    this._audioExtractor = this.GetAudioWriter( mediaInfo );
                    this.ExtractedAudio = this._audioExtractor != null;
                }

                if ( this._audioExtractor == null ) {
                    throw new InvalidOperationException( "No supported audio writer found." );
                }

                this._audioExtractor.WriteChunk( data, timeStamp );
            }

            return true;
        }

        private uint ReadUInt24() {
            var x = new byte[ 4 ];

            this._fileStream.Read( x, 1, 3 );
            this._fileOffset += 3;

            return BigEndianBitConverter.ToUInt32( x, 0 );
        }

        private byte[] ReadBytes( int length ) {
            var buff = new byte[ length ];

            this._fileStream.Read( buff, 0, length );
            this._fileOffset += length;

            return buff;
        }

        private IAudioExtractor GetAudioWriter( uint mediaInfo ) {
            var format = mediaInfo >> 4;

            switch ( format ) {
                case 14:
                case 2:
                    return new Mp3AudioExtractor( this._outputPath );

                case 10:
                    return new AacAudioExtractor( this._outputPath );
            }

            string typeStr;

            switch ( format ) {
                case 1:
                    typeStr = "ADPCM";
                    break;

                case 6:
                case 5:
                case 4:
                    typeStr = "Nellymoser";
                    break;

                default:
                    typeStr = "format=" + format;
                    break;
            }

            throw new AudioExtractionException( "Unable to extract audio (" + typeStr + " is unsupported)." );
        }

        public event EventHandler<ProgressEventArgs> ConversionProgressChanged;
    }
}