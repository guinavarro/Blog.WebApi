CREATE TABLE users (
    id bigserial PRIMARY KEY,
    created_at timestamptz NOT NULL,
    key uuid NOT NULL,
    username varchar NOT NULL,
    email varchar NOT NULL,
    passwordHash varchar NOT NULL
);

CREATE TABLE tags (
    id bigserial PRIMARY KEY,
    created_at timestamptz NOT NULL,
    key uuid NOT NULL,
    name varchar NOT NULL
);

CREATE TABLE posts (
    id bigserial PRIMARY KEY,
    created_at timestamptz NOT NULL,
    key uuid NOT NULL,
    title varchar NOT NULL,
    content varchar NOT NULL,
    active bool DEFAULT true,
    user_id int8 REFERENCES users(id) NOT NULL
);

CREATE TABLE tagspost (
    id bigserial PRIMARY KEY,
    created_at timestamptz NOT NULL,
    key uuid NOT NULL,
    post_id int8 REFERENCES posts(id) NOT NULL,
    tag_id int8 REFERENCES tags(id) NOT NULL
);

CREATE TABLE multimedia (
    id bigserial PRIMARY KEY,
    created_at timestamptz NOT NULL,
    key uuid NOT NULL,
    post_id int8 REFERENCES posts(id) NOT NULL,
    bucket_url varchar NOT NULL,
    file_name varchar NOT NULL,
    file_extension varchar NOT NULL,
    file_size_kb float8 NOT NULL
);
