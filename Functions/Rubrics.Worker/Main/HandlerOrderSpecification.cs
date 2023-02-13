using Rubrics.Handlers.Items;
using Rubrics.Handlers.MOSequences;
using Rubrics.Handlers.TestCases;
using System;
using System.Collections.Generic;
using Rubrics.Handlers.RubricReviewForwardEvents;
using Rubrics.Handlers.RubricReviews;

namespace Rubrics.Worker.Main
{
    public static class HandlerOrderSpecification
    {
        public static IEnumerable<Type> GetHandlerOrder()
        {
            return new[]
            {
                typeof(AddItemOnQuestionItemAdded),
                typeof(CreateRubricReviewOnRubricItemAdded), typeof(CreateMoSequenceOnRubricItemAdded), typeof(CreateRequiredTestCasesOnRubricItemAdded),
                typeof(ReconcileOnItemModified),
                typeof(RevokeAllApprovalsOnItemModified),
                typeof(UpdateTestCasesOnItemReconciled),
                typeof(UpdateMoSequenceOnMoAdded), typeof(UpdateTestCasesOnAuthoredMoAdded),
                typeof(UpdateMoSequenceOnMoDeleted), typeof(UpdateTestCasesOnAuthoredMoDeleted),
                typeof(UpdateTestCasesOnKeyOrAssertionEvent),
                typeof(UpdateRubricReviewOnTestCaseExecuted),
                typeof(UpdateRubricReviewOnTestCaseNeedsExecution),
                typeof(ExecuteTestCaseForMcqOnTestCaseNeedsExecution),
                typeof(UpdateRequiredTestCasesForMcqOnItemOrKeyOrAssertionOrMoEvent),
                typeof(UpdateStatusOnInventoryStatusChangedToPending),
                typeof(UpdateStatusOnInventoryStatusChangedToPostPending),
                typeof(ForwardRubricReviewPendingHandler),
                typeof(ForwardRubricReviewCompletedHandler)
            };
        }
    }
}
