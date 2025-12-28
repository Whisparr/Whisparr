interface ElementClass {
  add: (className: string) => void;
  remove: (className: string) => void;
}
declare module 'element-class' {
  function elementClass(element: HTMLElement): ElementClass;

  export = elementClass;
}
