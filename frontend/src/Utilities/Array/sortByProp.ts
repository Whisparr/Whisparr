export function sortByProp<T, K extends keyof T & string>(sortKey: K) {
  return (a: T, b: T): number => {
    const va = a[sortKey];
    const vb = b[sortKey];

    const sa = va === undefined || va === null ? '' : String(va);
    const sb = vb === undefined || vb === null ? '' : String(vb);

    return sa.localeCompare(sb, undefined, { numeric: true });
  };
}

export default sortByProp;
