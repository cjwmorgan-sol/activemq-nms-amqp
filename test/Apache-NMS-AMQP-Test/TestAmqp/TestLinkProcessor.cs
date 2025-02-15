﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using Amqp;
using Amqp.Framing;
using Amqp.Listener;
using Amqp.Types;

namespace NMS.AMQP.Test.TestAmqp
{
    public class TestLinkProcessor : ILinkProcessor
    {
        private List<Amqp.Message> messages;

        public TestLinkProcessor(List<Amqp.Message> messages)
        {
            this.messages = messages;
        }
        
        public TestLinkProcessor()
        {
            this.messages = new List<Amqp.Message>();
        }

        public AttachContext Consumer { get; private set; }
        public AttachContext Producer { get; private set; }

        public void Process(AttachContext attachContext)
        {
            if (!attachContext.Link.Role)
            {
                Consumer = attachContext;
                attachContext.Link.AddClosedCallback((sender, error) => { Consumer = null; });
            }
            else
            {
                Producer = attachContext;
                attachContext.Link.AddClosedCallback((sender, error) =>
                {
                    Producer = null;
                });
            }

            attachContext.Complete(new TestLinkEndpoint(messages), attachContext.Attach.Role ? 0 : 30);
        }

        public void CloseConsumer()
        {
            Consumer?.Link.Close();
        }

        public void CloseConsumerWithError()
        {
            Consumer?.Link.Close(TimeSpan.FromMilliseconds(1000), new Error(new Symbol(ErrorCode.DetachForced)));
        }
    }
}