import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function SeriesMonitorNewItemsOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('MonitorAllEpisodes')}
        data="Monitor all new episodes"
      />

      <DescriptionListItem
        title={translate('MonitorNone')}
        data="Don't monitor any new episodes"
      />
    </DescriptionList>
  );
}

export default SeriesMonitorNewItemsOptionsPopoverContent;
