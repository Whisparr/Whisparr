import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';

function ImportListMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title="All Site Episodes"
        data="Monitor every episode on each site added from the list"
      />

      <DescriptionListItem
        title="Specific Episode(s)"
        data="Monitor only the specific episodes from the list, and ignore all others"
      />

      <DescriptionListItem
        title="None"
        data="No episodes will be monitored when sites are added from the list"
      />
    </DescriptionList>
  );
}

export default ImportListMonitoringOptionsPopoverContent;
