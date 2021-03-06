﻿#region Copyright & License

// Copyright © 2012 - 2020 François Chabot
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.BizTalk.ContextProperties.Extensions;
using Be.Stateless.BizTalk.Message.Extensions;
using Be.Stateless.Extensions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace Be.Stateless.BizTalk.MicroComponent
{
	/// <summary>
	/// Propagates message type and correlation id over Azure ServiceBus queues inwards and outwards.
	/// </summary>
	/// <remarks>
	/// <para>
	/// For inbound messages, <see cref="SBMessagingProperties.CorrelationId">SBMessagingProperties.CorrelationId</see> and <see
	/// cref="BizTalkFactoryProperties.MessageType">BizTalkFactoryProperties.MessageType</see>, if any, are respectively
	/// promoted into BizTalk message context as <see
	/// cref="BizTalkFactoryProperties.CorrelationId">BizTalkFactoryProperties.CorrelationId</see> and <see
	/// cref="BtsProperties.MessageType">BtsProperties.MessageType</see>.
	/// </para>
	/// <para>
	/// For outbound messages, <see cref="BizTalkFactoryProperties.CorrelationId">BizTalkFactoryProperties.CorrelationId</see>
	/// and <see cref="BtsProperties.MessageType">BtsProperties.MessageType</see>, if any, are respectively propagated as
	/// <see cref="SBMessagingProperties.CorrelationId">SBMessagingProperties.CorrelationId</see>
	/// and <see cref="BizTalkFactoryProperties.MessageType">BizTalkFactoryProperties.MessageType</see>.
	/// </para>
	/// </remarks>
	public class SBMessagingContextPropagator : IMicroComponent
	{
		#region IMicroComponent Members

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (message.Direction().IsInbound())
			{
				var correlationId = message.GetProperty(SBMessagingProperties.CorrelationId);
				if (!correlationId.IsNullOrEmpty()) message.PromoteCorrelationId(correlationId);
				var messageType = message.GetProperty(BizTalkFactoryProperties.MessageType);
				if (!messageType.IsNullOrEmpty()) message.Promote(BtsProperties.MessageType, messageType);
			}
			else
			{
				var correlationToken = message.GetProperty(BizTalkFactoryProperties.CorrelationId);
				if (!correlationToken.IsNullOrEmpty()) message.SetProperty(SBMessagingProperties.CorrelationId, correlationToken);
				var messageType = message.GetOrProbeMessageType(pipelineContext);
				if (!messageType.IsNullOrEmpty()) message.SetProperty(BizTalkFactoryProperties.MessageType, messageType);
			}
			return message;
		}

		#endregion
	}
}
