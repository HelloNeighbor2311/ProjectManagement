$(function () {
    var l = abp.localization.getResource('ProjectManagement');
    var listUrl = abp.appPath + 'api/app/work-task/work-task';

    var $loading = $('#WorkTaskLoading');
    var $empty = $('#WorkTaskEmpty');
    var $error = $('#WorkTaskError');
    var $errorMessage = $('#WorkTaskErrorMessage');
    var $tableWrapper = $('#WorkTaskTableWrapper');

    var $search = $('#WorkTaskSearch');
    var $searchButton = $('#WorkTaskSearchButton');
    var $filterToggle = $('#WorkTaskFilterToggle');
    var $filters = $('#WorkTaskFilters');
    var $filterStatus = $('#WorkTaskFilterStatus');
    var $filterPriority = $('#WorkTaskFilterPriority');
    var $filterAssignee = $('#WorkTaskFilterAssignee');
    var $filterProject = $('#WorkTaskFilterProject');
    var createModal = new abp.ModalManager(
        abp.appPath + 'WorkTasks/CreateModal',
    );
    var editModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'WorkTasks/EditModal',
    });

    var allTasks = [];
    var dataTable = $('#WorkTaskTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: false,
            paging: true,
            searching: false,
            processing: false,
            order: [[1, 'asc']],
            data: [],
            columnDefs: [
                {
                    title: l('Actions'),
                    data: null,
                    width: '120px',
                    orderable: false,
                    render: function (data, type, row) {
                        return (
                            '<div class="dropdown">' +
                            '<button class="worktasks-action-btn dropdown-toggle" type="button" data-bs-toggle="dropdown" aria-expanded="false">' +
                            '<i class="fa fa-cog"></i>' +
                            '<span>' +
                            l('Actions') +
                            '</span>' +
                            '</button>' +
                            '<ul class="dropdown-menu dropdown-menu-end">' +
                            '<li>' +
                            '<a href="#" class="dropdown-item worktasks-edit-action" data-id="' +
                            row.id +
                            '">' +
                            '<i class="fa fa-pencil-alt me-2"></i>' +
                            l('Edit') +
                            '</a>' +
                            '</li>' +
                            '<li>' +
                            '<a href="#" class="dropdown-item worktasks-delete-action text-danger" data-id="' +
                            row.id +
                            '">' +
                            '<i class="fa fa-trash-alt me-2"></i>' +
                            l('Delete') +
                            '</a>' +
                            '</li>' +
                            '</ul>' +
                            '</div>'
                        );
                    },
                },
                {
                    title: l('Title'),
                    data: 'title',
                    defaultContent: '-',
                },
                {
                    title: l('Project'),
                    data: 'projectName',
                    defaultContent: '-',
                },
                {
                    title: l('Status'),
                    data: 'statusName',
                    render: function (data) {
                        return buildBadge(data, 'status')[0].outerHTML;
                    },
                    defaultContent: '-',
                },
                {
                    title: l('Priority'),
                    data: 'priorityName',
                    render: function (data) {
                        return buildBadge(data, 'priority')[0].outerHTML;
                    },
                    defaultContent: '-',
                },
                {
                    title: l('Assignee'),
                    data: 'assigneeName',
                    defaultContent: '-',
                },
                {
                    title: l('StartDate'),
                    data: 'startedDate',
                    render: function (data) {
                        return formatDate(data);
                    },
                    defaultContent: '-',
                },
                {
                    title: l('EndDate'),
                    data: 'endedDate',
                    render: function (data) {
                        return formatDate(data);
                    },
                    defaultContent: '-',
                },
            ],
        }),
    );

    $filterToggle.on('click', function () {
        var isOpen = !$filters.hasClass('d-none');
        $filters.toggleClass('d-none', isOpen);
        $(this)
            .find('i')
            .toggleClass('fa-chevron-up', !isOpen)
            .toggleClass('fa-chevron-down', isOpen);
    });

    $('#RetryWorkTasksButton').on('click', function (e) {
        e.preventDefault();
        loadTasks();
    });

    $searchButton.on('click', function () {
        renderTasks();
    });

    $search.on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            renderTasks();
        }
    });

    $search.on('input search', function () {
        renderTasks();
    });

    $filterStatus.on('change', renderTasks);
    $filterPriority.on('change', renderTasks);
    $filterAssignee.on('change', renderTasks);
    $filterProject.on('change', renderTasks);

    $('#NewWorkTaskButton').on('click', function (e) {
        e.preventDefault();
        createModal.open();
    });

    document.addEventListener('workTaskCreated', function () {
        loadTasks();
    });

    document.addEventListener('workTaskUpdated', function () {
        loadTasks();
    });

    $('#WorkTaskTable').on('click', '.worktasks-edit-action', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        if (!id) {
            return;
        }

        editModal.open({ id: id });
    });

    // Delete handler
    $('#WorkTaskTable').on('click', '.worktasks-delete-action', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        if (!id) {
            return;
        }

        abp.message.confirm(
            l('TaskDeleteConfirmationMessage'),
            l('Delete'),
            function (confirmed) {
                if (!confirmed) {
                    return;
                }

                abp.ajax({
                    url: abp.appPath + 'api/app/work-task/' + id + '/work-task',
                    type: 'DELETE',
                })
                    .done(function () {
                        abp.notify.info(l('SuccessfullyDeleted'));
                        // remove from local list and re-render
                        allTasks = allTasks.filter(function (t) {
                            return t.id !== id;
                        });
                        renderTasks();
                    })
                    .fail(function (error) {
                        var message =
                            error &&
                            error.responseJSON &&
                            error.responseJSON.error &&
                            error.responseJSON.error.message
                                ? error.responseJSON.error.message
                                : l('CouldNotDelete');
                        abp.notify.error(message);
                    });
            },
        );
    });

    loadTasks();

    function loadTasks() {
        setState('loading');

        abp.ajax({
            type: 'GET',
            url: listUrl,
        })
            .done(function (response) {
                allTasks = Array.isArray(response) ? response : [];
                buildFilterOptions(allTasks);
                renderTasks();
            })
            .fail(function (error) {
                var message =
                    error &&
                    error.responseJSON &&
                    error.responseJSON.error &&
                    error.responseJSON.error.message
                        ? error.responseJSON.error.message
                        : l('CouldNotLoadTasks');

                $errorMessage.text(message);
                setState('error');
            });
    }

    function setState(state) {
        $loading.toggleClass('d-none', state !== 'loading');
        $empty.toggleClass('d-none', state !== 'empty');
        $error.toggleClass('d-none', state !== 'error');
        $tableWrapper.toggleClass('d-none', state !== 'content');
    }

    function renderTasks() {
        var filtered = applyFilters(allTasks);

        dataTable.clear();

        if (!filtered.length) {
            dataTable.draw();
            setState('empty');
            return;
        }

        dataTable.rows.add(filtered).draw();
        setState('content');
    }

    function applyFilters(items) {
        var keyword = ($search.val() || '').trim().toLowerCase();
        var status = $filterStatus.val() || '';
        var priority = $filterPriority.val() || '';
        var assignee = $filterAssignee.val() || '';
        var project = $filterProject.val() || '';

        return items.filter(function (task) {
            if (keyword) {
                var haystack =
                    (task.title || '') +
                    ' ' +
                    (task.projectName || '') +
                    ' ' +
                    (task.statusName || '') +
                    ' ' +
                    (task.priorityName || '') +
                    ' ' +
                    (task.assigneeName || '');

                if (!haystack.toLowerCase().includes(keyword)) {
                    return false;
                }
            }

            if (status && (task.statusName || '') !== status) {
                return false;
            }

            if (priority && (task.priorityName || '') !== priority) {
                return false;
            }

            if (assignee && (task.assigneeName || '') !== assignee) {
                return false;
            }

            if (project && (task.projectName || '') !== project) {
                return false;
            }

            return true;
        });
    }

    function buildFilterOptions(items) {
        var statuses = uniqueValues(
            items.map(function (x) {
                return x.statusName;
            }),
        );
        var priorities = uniqueValues(
            items.map(function (x) {
                return x.priorityName;
            }),
        );
        var assignees = uniqueValues(
            items.map(function (x) {
                return x.assigneeName;
            }),
        );
        var projects = uniqueValues(
            items.map(function (x) {
                return x.projectName;
            }),
        );

        fillSelect($filterStatus, statuses, l('All'));
        fillSelect($filterPriority, priorities, l('All'));
        fillSelect($filterAssignee, assignees, l('All'));
        fillSelect($filterProject, projects, l('All'));
    }

    function fillSelect($select, values, allLabel) {
        $select.empty();
        $select.append($('<option/>', { value: '', text: allLabel || 'All' }));
        values.forEach(function (value) {
            $select.append($('<option/>', { value: value, text: value }));
        });
    }

    function uniqueValues(values) {
        return values
            .map(function (value) {
                return value || '';
            })
            .filter(function (value) {
                return value !== '';
            })
            .filter(function (value, index, array) {
                return array.indexOf(value) === index;
            })
            .sort();
    }

    function buildBadge(value, type) {
        var text = value || '-';
        var key = (value || '').toLowerCase().replace(/\s+/g, '');
        var className = 'worktasks-badge ' + type + '-default';

        if (type === 'status') {
            if (key === 'todo') {
                className = 'worktasks-badge status-todo';
            } else if (key === 'inprogress' || key === 'in-progress') {
                className = 'worktasks-badge status-inprogress';
            } else if (key === 'review') {
                className = 'worktasks-badge status-review';
            } else if (key === 'done') {
                className = 'worktasks-badge status-done';
            }
        }

        if (type === 'priority') {
            if (key === 'low') {
                className = 'worktasks-badge priority-low';
            } else if (key === 'medium') {
                className = 'worktasks-badge priority-medium';
            } else if (key === 'high') {
                className = 'worktasks-badge priority-high';
            } else if (key === 'critical') {
                className = 'worktasks-badge priority-critical';
            }
        }

        return $('<span/>', { class: className }).text(text);
    }

    function formatDate(value) {
        if (!value) {
            return '-';
        }

        var date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return value;
        }

        return date.toLocaleDateString();
    }
});
