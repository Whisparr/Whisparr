import classNames from 'classnames';
import React from 'react';
import styles from './DateInput.css';

interface ChangePayload {
  name: string;
  value: React.InputHTMLAttributes<HTMLInputElement>['value'];
}

interface Props {
  className?: string;
  type?: string;
  readOnly?: boolean;
  autoFocus?: boolean;
  placeholder?: string;
  name: string;
  value: React.InputHTMLAttributes<HTMLInputElement>['value'];
  hasError?: boolean;
  hasWarning?: boolean;
  hasButton?: boolean;
  onChange: (payload: ChangePayload) => void;
  onFocus?: (e: React.FocusEvent<HTMLInputElement>) => void;
  onBlur?: (e: React.FocusEvent<HTMLInputElement>) => void;
  onCopy?: (e: React.ClipboardEvent<HTMLInputElement>) => void;
  onSelectionChange?: (start: number | null, end: number | null) => void;
}

class DateInput extends React.Component<Props> {
  static defaultProps: Partial<Props> = {
    className: styles.input,
    type: 'date',
    readOnly: false,
    autoFocus: false,
    value: '',
  };

  componentDidMount() {
    window.addEventListener('mouseup', this.onDocumentMouseUp);
  }

  componentWillUnmount() {
    window.removeEventListener('mouseup', this.onDocumentMouseUp);

    if (this._selectionTimeout) {
      window.clearTimeout(this._selectionTimeout);
      this._selectionTimeout = null;
    }
  }

  private _input: HTMLInputElement | null = null;
  private _selectionStart: number | null = null;
  private _selectionEnd: number | null = null;
  private _selectionTimeout: number | null = null;
  private _isMouseTarget = false;

  setInputRef = (ref: HTMLInputElement | null) => {
    this._input = ref;
  };

  selectionChange() {
    if (this._selectionTimeout) {
      window.clearTimeout(this._selectionTimeout);
    }

    this._selectionTimeout = window.setTimeout(() => {
      const selectionStart = this._input ? this._input.selectionStart : null;
      const selectionEnd = this._input ? this._input.selectionEnd : null;

      const selectionChanged =
        this._selectionStart !== selectionStart ||
        this._selectionEnd !== selectionEnd;

      this._selectionStart = selectionStart;
      this._selectionEnd = selectionEnd;

      if (this.props.onSelectionChange && selectionChanged) {
        this.props.onSelectionChange(selectionStart, selectionEnd);
      }
    }, 10);
  }

  onChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name, onChange } = this.props;

    const payload: ChangePayload = {
      name,
      value: event.target.value,
    };

    onChange(payload);
  };

  onFocus = (event: React.FocusEvent<HTMLInputElement>) => {
    if (this.props.onFocus) {
      this.props.onFocus(event);
    }

    this.selectionChange();
  };

  onKeyUp = () => {
    this.selectionChange();
  };

  onMouseDown = () => {
    this._isMouseTarget = true;
  };

  onMouseUp = () => {
    this.selectionChange();
  };

  onDocumentMouseUp = () => {
    if (this._isMouseTarget) {
      this.selectionChange();
    }

    this._isMouseTarget = false;
  };

  // noop wheel handler to match original behaviour (JS referenced onWheel but didn't define it)
  onWheel = (_e: React.WheelEvent<HTMLInputElement>) => {
    // Intentionally empty to preserve original behavior
  };

  render() {
    const {
      className,
      type,
      readOnly,
      autoFocus,
      placeholder,
      name,
      value,
      hasError,
      hasWarning,
      hasButton,
      onBlur,
      onCopy,
    } = this.props;

    return (
      <input
        ref={this.setInputRef}
        type={type}
        readOnly={readOnly}
        autoFocus={autoFocus}
        placeholder={placeholder}
        className={classNames(
          className,
          readOnly && styles.readOnly,
          hasError && styles.hasError,
          hasWarning && styles.hasWarning,
          hasButton && styles.hasButton
        )}
        name={name}
        value={value}
        onChange={this.onChange}
        onFocus={this.onFocus}
        onBlur={onBlur}
        onCopy={onCopy}
        onCut={onCopy}
        onKeyUp={this.onKeyUp}
        onMouseDown={this.onMouseDown}
        onMouseUp={this.onMouseUp}
        onWheel={this.onWheel}
      />
    );
  }
}

export default DateInput;
