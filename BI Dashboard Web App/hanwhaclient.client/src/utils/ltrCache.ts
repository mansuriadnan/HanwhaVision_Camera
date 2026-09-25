import createCache from "@emotion/cache";

const ltrCache = createCache({
  key: "mui-ltr",
  prepend: true
});

export default ltrCache;
