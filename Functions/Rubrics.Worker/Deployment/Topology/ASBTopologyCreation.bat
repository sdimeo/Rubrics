dotnet tool install -g "NServiceBus.Transport.AzureServiceBus.CommandLine"

set environmentName=%~1
set sbConnectionString=%~2
set queueSizeInGB=%~3
set endpointName=%environmentName%-rubrics-worker-core

echo --Queue Setup--
asb-transport queue create %environmentName%-audit -s %queueSizeInGB% -c %sbConnectionString%
asb-transport queue create %environmentName%-error -s %queueSizeInGB% -c %sbConnectionString%

echo --Endpoint Setup--
asb-transport endpoint create %endpointName% -s %queueSizeInGB% -t %environmentName% -c %sbConnectionString%

echo --Subscription Setup--
asb-transport endpoint subscribe %endpointName% Rubrics.Domain.Assertions.AssertionEvent -r AssertionEvent -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% QuestionEditor.Contracts.Events.ItemAdded -r ItemAdded -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% QuestionEditor.Contracts.Events.ItemModified -r ItemModified -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Domain.Items.ItemReconciled -r ItemReconciled -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% ItOps.ItemImport.Contracts.ItemToImport -r ItemToImport -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Domain.Keys.KeyEvent -r KeyEvent -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Contracts.MeasurementOpportunities.MeasurementOpportunityAdded -r MeasurementOpportunityAdded -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Contracts.MeasurementOpportunities.MeasurementOpportunityDeleted -r MeasurementOpportunityDeleted -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Contracts.MeasurementOpportunities.MeasurementOpportunityEvent -r MeasurementOpportunityEvent -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Domain.Items.RubricItemAdded -r RubricItemAdded -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Contracts.RubricReviews.RubricReviewCompleted -r RubricReviewCompleted -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Contracts.RubricReviews.RubricReviewPending -r RubricReviewPending -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Inventory.Contracts.Events.StatusUpdatedToPending -r StatusUpdatedToPending -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Inventory.Contracts.Events.StatusUpdatedToPostPending -r StatusUpdatedToPostPending -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Domain.TestCases.TestCaseExecuted -r TestCaseExecuted -t %environmentName% -c %sbConnectionString%
asb-transport endpoint subscribe %endpointName% Rubrics.Domain.TestCases.TestCaseNeedsExecution -r TestCaseNeedsExecution -t %environmentName% -c %sbConnectionString%
