# Accessibility Info

This application has been tested for accessibility issues using the
WAVE browser plugin tool.

## Known Issues

### WAVE Warning: Redundant title text

The PagedRecordset component has some text that can be hidden if it
gets too long, such as the Title text on the requests listing.
That text uses a title html element that is the same as
the text shown on the page. However, the visual title may be
truncated and appended with an elipses. In those cases, hovering
the mouse over the text shows the full title. However, WAVE
reports a warning of duplicate title text. This has to stay in
place as even though the text and the title text are the same,
the visual text is not always the same as the title text
depending on browser size and length of the text.
This is because the CSS for those elements sets the inside
div as "overflow:hidden" with "text-overflow: ellipsis".

This same issue is seen in the ckEditor HTML editor
which has several toolbar menu icons that uses the same text
on the control and as the title element for the control.