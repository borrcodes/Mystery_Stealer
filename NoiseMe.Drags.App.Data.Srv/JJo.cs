using NoiseMe.Drags.App.Models;
using NoiseMe.Drags.App.Models.Communication;
using NoiseMe.Drags.App.Models.LocalModels.Extensions;
using NoiseMe.Drags.App.Models.WebSocket4Net;
using System;
using System.Threading;

namespace NoiseMe.Drags.App.Data.Srv
{
	public static class JJo
	{
		public static hd OnResponseRecieved;

		public static bool? OnResponse(this ResponseBase response)
		{
			return OnResponseRecieved?.Invoke(response);
		}

		public static AsyncAction ProcessResponse<TInput>(this RequestBase request, WebSocket session, TInput objectToSend)
		{
			return AsyncTask.StartNew(delegate
			{
				byte[] array = request.CreateResponse(objectToSend).SerializeProto();
				session.Send(array, 0, array.Length);
			});
		}

		public static AsyncAction ProcessRequest<TInput>(this WebSocket session, TInput objectToSend, string Name)
		{
			return AsyncTask.StartNew(delegate
			{
				byte[] array = new Request<TInput>(objectToSend, Name).SerializeProto();
				session.Send(array, 0, array.Length);
			});
		}

		public static TaskAction<TResult> ProcessRequest<TInput, TResult>(this WebSocket session, TInput objectToSend, string Name, int timeoutMilliseconds = 60000)
		{
			Request<TInput> localRequest = default(Request<TInput>);
			bool handled = default(bool);
			TResult result = default(TResult);
			return AsyncTask.StartNew(delegate
			{
				try
				{
					DateTime t = DateTime.Now.AddMilliseconds(timeoutMilliseconds);
					localRequest = new Request<TInput>(objectToSend, Name);
					handled = false;
					result = default(TResult);
					OnResponseRecieved = (hd)Delegate.Combine(OnResponseRecieved, new hd(OnResponse));
					byte[] array = localRequest.SerializeProto();
					session.Send(array, 0, array.Length);
					while (t >= DateTime.Now && !handled)
					{
						Thread.Sleep(100);
					}
					OnResponseRecieved = (hd)Delegate.Remove(OnResponseRecieved, new hd(OnResponse));
					return result;
				}
				catch (Exception value)
				{
					Console.WriteLine(value);
					return default(TResult);
				}
			});
		}
	}
}
