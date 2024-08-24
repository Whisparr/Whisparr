import React from 'react';
import Link, { LinkProps } from 'Components/Link/Link';

interface MovieTitleLinkProps extends LinkProps {
  foreignId: string;
  title: string;
  year?: number;
}

function MovieTitleLink({
  foreignId,
  title,
  year = 0,
  ...otherProps
}: MovieTitleLinkProps) {
  const link = `/movie/${foreignId}`;

  return (
    <Link to={link} title={title} {...otherProps}>
      {title}
      {year > 0 ? ` (${year})` : ''}
    </Link>
  );
}

export default MovieTitleLink;
