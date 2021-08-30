using Camera2.Behaviours;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Camera2.VMC {
	class OscClient : IDisposable {
		Socket _socket;

		public OscClient(IPEndPoint destination) {
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			if(destination.Address.ToString() == "255.255.255.255")
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

			_socket.Connect(destination);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/* Hardcoded for performance and simplicity because I probably will never need to send
         * any other commands / differently formatted ones
         */
		static readonly byte[] buf = Encoding.ASCII.GetBytes(
			"/VMC/Ext/Cam\0\0\0\0" +
			",sffffffff\0\0" +
			"Camera\0\0" +
			"XPOSYPOSZPOSXROTYROTZROTWROT_FOV"
		);

		public unsafe void SendCamPos(Cam2 camera) {
			fixed(byte* prt = buf) {
				uint rev(float x) {
					var value = (byte*)&x;

					return (uint)(*(value++) << 24) | (uint)(*(value++) << 16) | (uint)(*(value++) << 8) | *value;
				}

				var p = (uint*)(prt + buf.Length - (4 * 8));

				p[0] = rev(camera.transformchain.position.x);
				p[1] = rev(camera.transformchain.position.y);
				p[2] = rev(camera.transformchain.position.z);
				p[3] = rev(camera.transformchain.rotation.x);
				p[4] = rev(camera.transformchain.rotation.y);
				p[5] = rev(camera.transformchain.rotation.z);
				p[6] = rev(camera.transformchain.rotation.w);
				p[7] = rev(camera.settings.FOV);
			}

			_socket.Send(buf);
		}

		bool _disposed;

		void Dispose(bool disposing) {
			if(!_disposed && (_disposed = true) && disposing)
				_socket?.Close();
		}

		~OscClient() => Dispose(false);
	}
}
