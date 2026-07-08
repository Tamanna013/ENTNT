# FleetMind UI Component Library

This document serves as the central reference for the shared primitives in `src/components/ui/`. These components form the foundation of the FleetMind AI frontend, providing consistent styling, accessibility, and behavior across all feature modules.

When building a new page or feature, always reach for these primitives first before building custom UI elements.

---

## Data Display

### Table (`Table.tsx`)
A flexible, responsive data table with built-in sorting and mobile-friendly card fallback behavior. It accepts a generic array of data and a column definition array.

**Props:**
- `columns` (Required `Column<T>[]`): Array of column definitions (key, header, render function, sortable).
- `data` (Required `T[]`): The data array to render.
- `sortBy` (Optional `string`): The current sort key.
- `sortDescending` (Optional `boolean`): Sort direction.
- `onSortChange` (Optional `(key: string) => void`): Callback when a sortable header is clicked.
- `isLoading` (Optional `boolean`): Renders a loading state if true.
- `emptyMessage` (Optional `string`): Custom message when data array is empty.
- `mobileCardRenderer` (Optional `(row: T) => ReactNode`): Custom render function for mobile cards.

**Guidance:** 
`Table`'s `mobileCardRenderer` prop is optional. Omit it to use the automatic default card fallback, which intelligently renders the "Actions" column at the bottom of the card, or provide a custom render function for pages where the default doesn't produce an optimal layout.

**Example:**
```tsx
const columns = [
  { key: 'name', header: 'Name' },
  { key: 'status', header: 'Status', render: (row) => <Badge text={row.status} /> }
];
<Table data={items} columns={columns} isLoading={isLoading} />
```

### Badge (`Badge.tsx`)
A small contextual indicator used for statuses, types, and roles.

**Props:**
- `text` (Required `string`): The label to display.
- `color` (Optional `'blue' | 'green' | 'red' | 'gray' | 'yellow' | 'purple'`): The semantic color variant. Defaults to `gray`.

**Guidance:**
Always use semantic colors where possible (e.g. `green` for Active/Success, `red` for Error/Hazardous, `yellow` for Maintenance/Pending).

**Example:**
```tsx
<Badge text="Active" color="green" />
```

### Pagination (`Pagination.tsx`)
A standard numeric pagination control.

**Props:**
- `pageNumber` (Required `number`): The current 1-indexed page.
- `totalPages` (Required `number`): The total number of available pages.
- `onPageChange` (Required `(page: number) => void`): Callback when a new page is selected.

**Example:**
```tsx
<Pagination 
  pageNumber={query.pageNumber} 
  totalPages={Math.ceil(totalCount / query.pageSize)} 
  onPageChange={(p) => setQuery({...query, pageNumber: p})} 
/>
```

---

## Forms & Input

### Input (`Input.tsx`)
A standard accessible text input field with built-in label and error messaging.

**Props:**
- `label` (Required `string`): The visible label text.
- `error` (Optional `string`): Validation error message to display.
- `...props`: Standard `React.InputHTMLAttributes<HTMLInputElement>`.

**Guidance:**
Use standard React Hook Form integrations (e.g. `{...register('name')}`) directly on this component. The component handles `aria-invalid` and `aria-describedby` wiring automatically if an `error` prop is passed.

**Example:**
```tsx
<Input 
  label="Ship Name" 
  error={errors.name?.message} 
  {...register('name')} 
/>
```

### SearchInput (`SearchInput.tsx`)
A debounced input designed specifically for search bars.

**Props:**
- `value` (Required `string`): The current search string.
- `onChange` (Required `(value: string) => void`): Callback fired after the debounce interval.
- `placeholder` (Optional `string`): Input placeholder text.
- `debounceMs` (Optional `number`): Debounce interval. Defaults to 300ms.

**Example:**
```tsx
<SearchInput 
  value={searchTerm} 
  onChange={(val) => setQuery({ ...query, searchTerm: val })} 
  placeholder="Search ships..." 
/>
```

### DateRangePicker (`DateRangePicker.tsx`)
A two-input control for selecting a start and end date.

**Props:**
- `fromValue` (Required `string | undefined`): Start date string (YYYY-MM-DD).
- `toValue` (Required `string | undefined`): End date string.
- `onFromChange` (Required `(value: string | undefined) => void`): Start date callback.
- `onToChange` (Required `(value: string | undefined) => void`): End date callback.

**Example:**
```tsx
<DateRangePicker 
  fromValue={query.startDate} 
  toValue={query.endDate}
  onFromChange={(val) => setQuery({ ...query, startDate: val })}
  onToChange={(val) => setQuery({ ...query, endDate: val })}
/>
```

### FileUpload (`FileUpload.tsx`)
A drag-and-drop zone for file uploads.

**Props:**
- `onFileSelected` (Required `(file: File) => void`): Callback when a file is chosen or dropped.
- `accept` (Optional `string`): Mime-type accept string (e.g. `image/*`). Defaults to `*/*`.
- `isLoading` (Optional `boolean`): Disables the input and shows a spinner.

**Example:**
```tsx
<FileUpload 
  accept="image/*" 
  isLoading={isUploading} 
  onFileSelected={(file) => handleUpload(file)} 
/>
```

### FormError (`FormError.tsx`)
A small utility for rendering form-level (non-field-specific) error messages.

**Props:**
- `message` (Optional `string | null`): The error message. Renders nothing if empty.
- `id` (Optional `string`): HTML id for ARIA linking.

**Example:**
```tsx
<FormError message={submitError} />
```

---

## Feedback & Overlays

### Modal (`Modal.tsx`)
An accessible dialog overlay. Supports click-outside-to-close and escape-key handling.

**Props:**
- `isOpen` (Required `boolean`): Controls visibility.
- `onClose` (Required `() => void`): Callback to request closure.
- `title` (Required `string`): The modal header title.
- `children` (Required `ReactNode`): The modal content.

**Guidance:**
Modal widths automatically adapt to the viewport (`max-w-full sm:max-w-lg`). Vertical overflow triggers a scrollbar inside the modal rather than breaking the page.

**Example:**
```tsx
<Modal isOpen={isOpen} onClose={() => setIsOpen(false)} title="Add Ship">
  <ShipForm onSubmit={handleSave} />
</Modal>
```

### Toast (`Toast.tsx`)
Global notification system. Consists of a Provider and the Toast elements.

**Props (`ToastProps`):**
- `toast` (Required `ToastMessage`): The toast data (id, message, type: 'success' | 'error' | 'info').
- `onRemove` (Required `(id: string) => void`): Callback to dismiss the toast.

**Guidance:**
Do not render `<Toast>` directly. Wrap the app in `<ToastProvider>` and use the `useToast()` hook to trigger notifications programmatically.

**Example:**
```tsx
const { showToast } = useToast();
showToast('Saved successfully', 'success');
```

---

## Actions

### Button (`Button.tsx`)
A standard button with loading and disabled states.

**Props:**
- `variant` (Optional `'primary' | 'secondary'`): The visual style. Defaults to `primary`.
- `isLoading` (Optional `boolean`): Disables the button and replaces the text with a spinner.
- `...props`: Standard `React.ButtonHTMLAttributes<HTMLButtonElement>`.

**Example:**
```tsx
<Button type="submit" isLoading={isSubmitting} variant="primary">
  Save Changes
</Button>
```

### ExportButton (`ExportButton.tsx`)
A standardized button that triggers a CSV/Excel export download from the backend.

**Props:**
- `exportPath` (Required `string`): The API endpoint path (e.g. `/cargo/export`).
- `filters` (Required `Record<string, unknown>`): The current query filters to append as query parameters.

**Example:**
```tsx
<ExportButton exportPath="/ships/export" filters={query} />
```

---

## Known, Accepted Inconsistencies

1. **`SearchInput` Prop Naming:** `SearchInput` uses `value` / `onChange`, whereas `DateRangePicker` uses `fromValue` / `onFromChange`. We decided not to unify these because `DateRangePicker` inherently deals with a dual-value tuple, making generic `value`/`onChange` ambiguous, whereas `SearchInput` naturally maps to a standard uncontrolled/controlled string input interface.
2. **FormModal Prop Spread (`ship` vs `cargo`):** Each `*FormModal` uses a domain-specific edit prop (e.g. `ship={ship}` or `cargo={cargo}`) rather than a generic `entity={item}` or `data={data}` prop. We preserved domain-specific naming to maximize type clarity within the component implementation itself, accepting the minor inconsistency across different form modals.
3. **`FormError` vs inline Input errors:** `FormError.tsx` exists purely for root-level form submission errors, while `Input.tsx` renders its own error text directly. We did not consolidate this into a single error component because field-level errors need tight coupling with their inputs for proper `aria-describedby` wiring.
