import React from 'react';
import Icon from 'Components/Icon';
import styles from './SceneIndexOverviewInfoRow.css';

interface SceneIndexOverviewInfoRowProps {
  title?: string;
  iconName: object;
  label: string;
}

function SceneIndexOverviewInfoRow(props: SceneIndexOverviewInfoRowProps) {
  const { title, iconName, label } = props;

  return (
    <div className={styles.infoRow} title={title}>
      <Icon className={styles.icon} name={iconName} size={14} />

      {label}
    </div>
  );
}

export default SceneIndexOverviewInfoRow;
