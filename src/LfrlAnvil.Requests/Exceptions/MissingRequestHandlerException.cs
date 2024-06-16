// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace LfrlAnvil.Requests.Exceptions;

/// <summary>
/// Represents an error due to missing <see cref="IRequestHandler{TRequest,TResult}"/> factory.
/// </summary>
public class MissingRequestHandlerException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="MissingRequestHandlerException"/> instance.
    /// </summary>
    /// <param name="requestType">Request type.</param>
    public MissingRequestHandlerException(Type requestType)
        : base( Resources.MissingRequestHandler( requestType ) )
    {
        RequestType = requestType;
    }

    /// <summary>
    /// Request type.
    /// </summary>
    public Type RequestType { get; }
}
