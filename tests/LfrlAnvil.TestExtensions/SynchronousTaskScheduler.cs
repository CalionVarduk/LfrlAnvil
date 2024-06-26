﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace LfrlAnvil.TestExtensions;

public sealed class SynchronousTaskScheduler : TaskScheduler
{
    protected override void QueueTask(Task task)
    {
        TryExecuteTask( task );
    }

    protected override bool TryExecuteTaskInline(Task task, bool wasPreviouslyQueued)
    {
        return TryExecuteTask( task );
    }

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        yield break;
    }
}
