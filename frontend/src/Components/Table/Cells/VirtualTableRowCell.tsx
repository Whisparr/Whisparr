import React from 'react';
import styles from './VirtualTableRowCell.css';

export interface VirtualTableRowCellProps {
  className?: string;
  children?: string | React.ReactNode;
  title?: string;
}

function VirtualTableRowCell({
  className = styles.cell,
  children,
  title = '',
}: VirtualTableRowCellProps) {
  return (
    <div className={className} title={title}>
      {children}
    </div>
  );
}

export default VirtualTableRowCell;
