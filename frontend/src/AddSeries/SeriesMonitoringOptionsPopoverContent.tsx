import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function SeriesMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('MonitorAllScenes')}
        data={translate('MonitorAllScenesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorFutureScenes')}
        data={translate('MonitorFutureScenesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorMissingScenes')}
        data={translate('MonitorMissingScenesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorExistingScenes')}
        data={translate('MonitorExistingScenesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorFirstYear')}
        data={translate('MonitorFirstYearDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorLatestYear')}
        data={translate('MonitorLatestYearDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorNone')}
        data={translate('MonitorNoneDescription')}
      />
    </DescriptionList>
  );
}

export default SeriesMonitoringOptionsPopoverContent;
